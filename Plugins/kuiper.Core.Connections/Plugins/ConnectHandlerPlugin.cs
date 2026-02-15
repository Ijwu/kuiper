using kbo.littlerocks;
using kbo.bigrocks;
using kuiper.Plugins;
using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;
using Microsoft.Extensions.Logging;
using kuiper.Core.Constants;

namespace kuiper.Core.Connections.Plugins
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
        private readonly INotifyingStorageService _storageService;
        private readonly IKuiperConfigService _kuiperConfig;

        public ConnectHandlerPlugin(ILogger<ConnectHandlerPlugin> logger,
                                    MultiData multiData,
                                    IConnectionManager connectionManager,
                                    ILocationCheckService locationCheckService,
                                    IHintPointsService hintPointsService,
                                    IServerAnnouncementService announcementService,
                                    INotifyingStorageService storageService,
                                    IKuiperConfigService kuiperConfig)
            : base(logger, connectionManager)
        {
            _multiData = multiData;
            _locationCheckService = locationCheckService;
            _hintPointsService = hintPointsService;
            _announcementService = announcementService;
            _storageService = storageService;
            _kuiperConfig = kuiperConfig;
        }

        protected override void RegisterHandlers()
        {
            Handle<Connect>(HandleConnectAsync);
        }

        private async Task HandleConnectAsync(Connect connectPacket, string connectionId)
        {
            Logger.LogDebug("Received Connect packet from client (ConnectionId: {ConnectionId}). Game: {Game}, Name: {Name}, Version: {Version}",
                connectionId,
                connectPacket.Game ?? "Unknown", 
                connectPacket.Name ?? "Unknown", 
                connectPacket.Version?.ToString() ?? "Unknown");

            // Validate the Connect packet
            ValidationResult validationResult = await ValidateConnectPacketAsync(connectPacket);

            if (!validationResult.IsValid)
            {
                // Send ConnectionRefused response only to origin connection
                var refusedPacket = new ConnectionRefused(validationResult.Error != null ? [validationResult.Error] : []);

                await SendToConnectionAsync(connectionId, refusedPacket);
                Logger.LogWarning("Connection refused for connection ({ConnectionId}): {Reason}", connectionId, validationResult.Error);

                await ConnectionManager.RemoveConnectionAsync(connectionId);

                return;
            }

            // If validation passed, attempt to map connectionId -> slot id (if a matching slot exists)
            int? matchedSlotId = ResolveSlotId(connectPacket.Name, connectPacket.Game);

            if (matchedSlotId.HasValue)
            {
                await ConnectionManager.MapConnectionToSlotAsync(connectionId, matchedSlotId.Value);
                Logger.LogInformation("Mapped connection {ConnectionId} to slot {Slot}", connectionId, matchedSlotId.Value);
            }
            else
            {
                var refusedPacket = new ConnectionRefused(["InvalidSlot"]);
                await SendToConnectionAsync(connectionId, refusedPacket);
                Logger.LogWarning("Could not map incoming connection ({ConnectionId}) to a slot in the multidata. Aborting connection.", connectionId);

                await ConnectionManager.RemoveConnectionAsync(connectionId);

                return;
            }

            // Build player list from MultiData slots
            List<NetworkPlayer> players = [];
            if (_multiData.SlotInfo != null)
            {
                foreach (var kvp in _multiData.SlotInfo)
                {
                    players.Add(new NetworkPlayer(0, kvp.Key, kvp.Value.Name, kvp.Value.Name));
                }
            }

            // Determine hint points (fallback to 0)
            long hintPoints = 0;
            if (matchedSlotId.HasValue)
            {
                hintPoints = await _hintPointsService.GetHintPointsAsync(matchedSlotId.Value);
            }

            // get slotdata for connecting client, if it's requested
            Dictionary<string, object>? slotDataForClient = null;
            if (matchedSlotId.HasValue && _multiData.SlotData != null && connectPacket.SlotData)
            {
                if (_multiData.SlotData.TryGetValue(matchedSlotId.Value, out var sd))
                {
                    slotDataForClient = new Dictionary<string, object>(sd);
                }
            }
            

            // SlotInfo: map local SlotInfo to library NetworkSlot dictionary
            Dictionary<long, NetworkObject> slotInfoDict;
            if (_multiData.SlotInfo != null)
            {
                slotInfoDict = _multiData.SlotInfo.ToDictionary(
                    kvp => (long)kvp.Key,
                    kvp => (NetworkObject)new NetworkSlot(kvp.Value.Name, kvp.Value.Game,(SlotType)(int)kvp.Value.Type, kvp.Value.GroupMembers.Cast<long>().ToArray()));
            }
            else
            {
                slotInfoDict = new Dictionary<long, NetworkObject>();
            }

            // LocationsChecked / MissingChecks: load recorded checks for the slot if available
            long[] locationsChecked = [];
            if (matchedSlotId.HasValue)
            {
                var checks = await _locationCheckService.GetChecksAsync(matchedSlotId.Value);
                locationsChecked = checks?.ToArray() ?? [];
            }

            long[] missingChecks = [];
            if (matchedSlotId.HasValue)
            {
                HashSet<long> allChecks = _multiData.Locations[matchedSlotId.Value].Keys.Select(i => (long)i).ToHashSet();
                HashSet<long> recordedChecks = locationsChecked.ToHashSet();
                missingChecks = allChecks.Except(recordedChecks).ToArray();
            }

            // Send Connected response populated with players and other fields
            var connectedPacket = new Connected(0, matchedSlotId!.Value, players.ToArray(), missingChecks, locationsChecked, slotDataForClient, slotInfoDict, hintPoints);

            // Send Connected only to the connecting client
            await SendToConnectionAsync(connectionId, connectedPacket);
            Logger.LogDebug("Sent Connected packet for client {Name} (ConnectionId: {ConnectionId})", connectPacket.Name ?? "Unknown", connectionId);

            if (matchedSlotId.HasValue)
            {
                await _announcementService.AnnouncePlayerConnectedAsync(matchedSlotId.Value, connectPacket.Name ?? _multiData.SlotInfo![matchedSlotId.Value].Name);
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
                }

                // Validate that the client name is specified
                if (string.IsNullOrWhiteSpace(packet.Name))
                {
                    Logger.LogDebug("Invalid or missing client name");
                    return new ValidationResult(false, "InvalidSlot");
                }

                // TODO: Validate version compatibility
                if (packet.Version == null)
                {
                    Logger.LogDebug("Invalid or missing version");
                    return new ValidationResult(false, "IncompatibleVersion");
                }

                // Verify the game exists in our slot configuration, skipping text clients
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
                    expectedPassword = await _storageService.LoadAsync<string>(StorageKeys.Password(slotId.Value));
                }

                if (string.IsNullOrEmpty(expectedPassword))
                {
                    // Fallback to server config
                    try
                    {
                         expectedPassword = _kuiperConfig.GetServerConfig<string>("Server:Password");
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
                         Logger.LogWarning("Invalid password for attempted connection to slot ({SlotId})", slotId);
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

        private int? ResolveSlotId(string? name, string? game)
        {
            if (string.IsNullOrWhiteSpace(name) || _multiData.SlotInfo == null)
            return null;

            var match = _multiData.SlotInfo.FirstOrDefault(kvp =>
                string.Equals(kvp.Value.Name, name) &&
                (string.IsNullOrWhiteSpace(game) || game == "Archipelago" || string.Equals(kvp.Value.Game, game))
            );

            return !match.Equals(default(KeyValuePair<int, MultiDataNetworkSlot>)) ? match.Key : null;
        }
    }
}
