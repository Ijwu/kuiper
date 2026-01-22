using kuiper.Services.Abstract;
using kuiper.Services;
using kuiper.Pickle;
using kbo.littlerocks;
using kbo.bigrocks;

namespace kuiper.Plugins
{
    /// <summary>
    /// Plugin that handles Connect packets from clients and responds with Connected or ConnectionRefused.
    /// </summary>
    public class ConnectHandlerPlugin : BasePlugin
    {
        private readonly MultiData _multiData;
        private readonly ILocationCheckService _locationCheckService;
        private readonly IHintPointsService _hintPointsService;
        private readonly IServerAnnouncementService _announcementService;
        private readonly IStorageService _storageService;
        private readonly IKuiperConfig _kuiperConfig;

        public ConnectHandlerPlugin(ILogger<ConnectHandlerPlugin> logger, MultiData multiData, WebSocketConnectionManager connectionManager, ILocationCheckService locationCheckService, IHintPointsService hintPointsService, IServerAnnouncementService announcementService, IStorageService storageService, IKuiperConfig kuiperConfig)
            : base(logger, connectionManager)
        {
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _locationCheckService = locationCheckService ?? throw new ArgumentNullException(nameof(locationCheckService));
            _hintPointsService = hintPointsService ?? throw new ArgumentNullException(nameof(hintPointsService));
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _kuiperConfig = kuiperConfig ?? throw new ArgumentNullException(nameof(kuiperConfig));
        }

        protected override void RegisterHandlers()
        {
            Handle<Connect>(HandleConnectAsync);
        }

        private async Task HandleConnectAsync(Connect connectPacket, string connectionId)
        {
            Logger.LogInformation("Received Connect packet from client (ConnectionId: {ConnectionId}). Game: {Game}, Name: {Name}, Version: {Version}",
                connectionId,
                connectPacket.Game ?? "Unknown", 
                connectPacket.Name ?? "Unknown", 
                connectPacket.Version?.ToString() ?? "Unknown");

            // Validate the Connect packet
            var validationResult = await ValidateConnectPacketAsync(connectPacket);

            if (!validationResult.IsValid)
            {
                // Send ConnectionRefused response only to origin connection
                var refusedPacket = new ConnectionRefused(new[] { validationResult.Error });

                await SendToConnectionAsync(connectionId, refusedPacket);
                Logger.LogWarning("Connection refused for client {Name} (ConnectionId: {ConnectionId}): {Reason}",
                    connectPacket.Name ?? "Unknown", connectionId, validationResult.Error);

                await ConnectionManager.RemoveConnectionAsync(connectionId);

                return;
            }

            // If validation passed, attempt to map connectionId -> slot id (if a matching slot exists)
            int? matchedSlotId = null;
            try
            {
                matchedSlotId = ResolveSlotId(connectPacket.Name, connectPacket.Game);

                if (matchedSlotId.HasValue)
                {
                    await ConnectionManager.MapConnectionToSlotAsync(connectionId, matchedSlotId.Value);
                    Logger.LogInformation("Mapped connection {ConnectionId} to slot {Slot}", connectionId, matchedSlotId.Value);
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Error mapping connection to slot; continuing without mapping.");
            }
            
            // Build player list from connections
            var allConnectionIds = ConnectionManager.GetAllConnectionIds();
            var players = new List<NetworkPlayer>();
            foreach (var connId in allConnectionIds)
            {
                try
                {
                    var slotId = await ConnectionManager.GetSlotForConnectionAsync(connId);
                    string playerName = "Unknown";
                    if (slotId.HasValue && _multiData.SlotInfo != null && _multiData.SlotInfo.TryGetValue((int)slotId.Value, out var slotInfo))
                    {
                        playerName = slotInfo.Name;
                    }
                    var player = new NetworkPlayer(0, slotId ?? 0, playerName, playerName);
                    players.Add(player);
                }
                catch (Exception ex)
                {
                    Logger.LogDebug(ex, "Error building player entry for connection {ConnectionId}; skipping.", connId);
                }
            }

            // Determine hint points (fallback to 0)
            var hintPoints = 0;
            if (matchedSlotId.HasValue)
            {
                try
                {
                    hintPoints = await _hintPointsService.GetHintPointsAsync(matchedSlotId.Value);
                }
                catch (Exception ex)
                {
                    Logger.LogDebug(ex, "Error retrieving hint points for slot {Slot}; defaulting to 0.", matchedSlotId.Value);
                    hintPoints = 0;
                }
            }

            // SlotData: attempt to use per-slot data from MultiData.SlotData if we can identify the connecting slot
            Dictionary<string, object>? slotDataForClient = null;
            try
            {
                if (matchedSlotId.HasValue && _multiData.SlotData != null && connectPacket.SlotData)
                {
                    if (_multiData.SlotData.TryGetValue(matchedSlotId.Value, out var sd))
                    {
                        slotDataForClient = new Dictionary<string, object>(sd);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Error extracting SlotData for client; falling back to empty SlotData.");
                slotDataForClient = null;
            }

            // SlotInfo: map local SlotInfo to library NetworkSlot dictionary
            Dictionary<long, NetworkObject> slotInfoDict;
            if (_multiData.SlotInfo != null)
            {
                slotInfoDict = _multiData.SlotInfo.ToDictionary(
                    kvp => (long)kvp.Key,
                    kvp => (NetworkObject)new NetworkSlot(kvp.Value.Name, kvp.Value.Game,(kbo.littlerocks.SlotType)(int)kvp.Value.Type, kvp.Value.GroupMembers.Cast<long>().ToArray()));
            }
            else
            {
                slotInfoDict = new Dictionary<long, NetworkObject>();
            }

            // LocationsChecked / MissingChecks: load recorded checks for the slot if available
            long[] locationsChecked;
            try
            {
                if (matchedSlotId.HasValue)
                {
                    var checks = await _locationCheckService.GetChecksAsync(matchedSlotId.Value);
                    locationsChecked = checks?.Select(i => (long)i).ToArray() ?? Array.Empty<long>();
                }
                else
                {
                    locationsChecked = Array.Empty<long>();
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Error loading recorded checks for slot; sending empty checks list.");
                locationsChecked = Array.Empty<long>();
            }

            var missingChecks = Array.Empty<long>();
            if (matchedSlotId.HasValue)
            { 
                var allChecks = _multiData.Locations[matchedSlotId.Value].Keys.Select(i => (long)i).ToHashSet();
                var recordedChecks = locationsChecked.ToHashSet();
                missingChecks = allChecks.Except(recordedChecks).ToArray();
            }

            // Send Connected response populated with players and other fields
            var connectedPacket = new Connected(0, (long)(matchedSlotId ?? 0), players.ToArray(), missingChecks, locationsChecked, slotDataForClient, slotInfoDict, hintPoints);

            // Send Connected only to the connecting client
            await SendToConnectionAsync(connectionId, connectedPacket);
            Logger.LogInformation("Sent Connected packet for client {Name} (ConnectionId: {ConnectionId})", connectPacket.Name ?? "Unknown", connectionId);

            if (matchedSlotId.HasValue)
            {
                await _announcementService.AnnouncePlayerConnectedAsync(matchedSlotId.Value, connectPacket.Name ?? _multiData.SlotInfo[matchedSlotId.Value].Name);
            }
        }

        /// <summary>
        /// Validation result for Connect packet validation.
        /// </summary>
        private record ValidationResult(bool IsValid, string? Error);

        /// <summary>
        /// Validates a Connect packet against server state and configuration.
        /// </summary>
        private async Task<ValidationResult> ValidateConnectPacketAsync(Connect packet)
        {
            try
            {
                // If Game is empty or whitespace, it's a Text Client — accept if a name is provided.
                if (string.IsNullOrWhiteSpace(packet.Game) || packet.Game == "Archipelago" )
                {
                    Logger.LogDebug("Empty game indicates a Text Client; skipping game/version checks.");

                    if (string.IsNullOrWhiteSpace(packet.Name))
                    {
                        Logger.LogDebug("Invalid or missing client name for Text Client");
                        return new ValidationResult(false, "InvalidSlot");
                    }
                }

                // Validate that the client name is specified
                if (string.IsNullOrWhiteSpace(packet.Name))
                {
                    Logger.LogDebug("Invalid or missing client name");
                    return new ValidationResult(false, "InvalidSlot");
                }

                // Validate version compatibility (if needed)
                if (packet.Version == null)
                {
                    Logger.LogDebug("Invalid or missing version");
                    return new ValidationResult(false, "IncompatibleVersion");
                }

                // Verify the game exists in our slot configuration
                // Note: Text clients skipped this check above.
                if (!string.IsNullOrWhiteSpace(packet.Game) && packet.Game != "Archipelago")
                {
                    var gameExists = _multiData.SlotInfo.Values.Any(slot => 
                        string.Equals(slot.Game, packet.Game, StringComparison.OrdinalIgnoreCase));

                    if (!gameExists)
                    {
                        Logger.LogDebug("Game not found in server configuration: {Game}", packet.Game);
                        return new ValidationResult(false, "InvalidGame");
                    }
                }

                // Password Check
                string? expectedPassword = null;
                int? slotId = ResolveSlotId(packet.Name, packet.Game);

                if (slotId.HasValue)
                {
                    expectedPassword = await _storageService.LoadAsync<string>($"#password:slot:{slotId}");
                }

                if (string.IsNullOrEmpty(expectedPassword))
                {
                    // Fallback to server config
                    try
                    {
                         // Using dynamic or map to class
                         var serverConfig = _kuiperConfig.GetServerConfig<ServerConfig>("Server");
                         expectedPassword = serverConfig?.Password;
                    }
                    catch 
                    {
                         // Config might be missing or section missing, ignore
                    }
                }

                if (!string.IsNullOrEmpty(expectedPassword))
                {
                    if (packet.Password != expectedPassword)
                    {
                         Logger.LogWarning("Invalid password for {Name}", packet.Name);
                         return new ValidationResult(false, "InvalidPassword");
                    }
                }

                return new ValidationResult(true, null);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Error during packet validation");
                return new ValidationResult(false, "IncompatibleVersion");
            }
        }

        private int? ResolveSlotId(string name, string? game)
        {
             if (string.IsNullOrWhiteSpace(name) || _multiData.SlotInfo == null)
                return null;

             var match = _multiData.SlotInfo.FirstOrDefault(kvp =>
                string.Equals(kvp.Value.Name, name, StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrWhiteSpace(game) || game == "Archipelago" || string.Equals(kvp.Value.Game, game, StringComparison.OrdinalIgnoreCase)));

             if (!match.Equals(default(KeyValuePair<int, Pickle.MultiDataNetworkSlot>)))
             {
                 return match.Key;
             }
             return null;
        }

        private record ServerConfig { public string? Password { get; set; } }
    }
}
