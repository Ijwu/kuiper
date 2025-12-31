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
    public class ConnectHandlerPlugin : IPlugin
    {
        private readonly ILogger<ConnectHandlerPlugin> _logger;
        private readonly MultiData _multiData;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly ILocationCheckService _locationCheckService;
        private readonly IHintPointsService _hintPointsService;

        public ConnectHandlerPlugin(ILogger<ConnectHandlerPlugin> logger, MultiData multiData, WebSocketConnectionManager connectionManager, ILocationCheckService locationCheckService, IHintPointsService hintPointsService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _locationCheckService = locationCheckService ?? throw new ArgumentNullException(nameof(locationCheckService));
            _hintPointsService = hintPointsService;
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            // Only handle Connect packets
            if (packet is not Connect connectPacket)
            {
                return;
            }

            try
            {
                _logger.LogInformation("Received Connect packet from client (ConnectionId: {ConnectionId}). Game: {Game}, Name: {Name}, Version: {Version}",
                    connectionId,
                    connectPacket.Game ?? "Unknown", 
                    connectPacket.Name ?? "Unknown", 
                    connectPacket.Version?.ToString() ?? "Unknown");

                // Validate the Connect packet
                var validationResult = ValidateConnectPacket(connectPacket);

                if (!validationResult.IsValid)
                {
                    // Send ConnectionRefused response only to origin connection
                    var refusedPacket = new ConnectionRefused(new[] { validationResult.Error });

                    await SendPacketToClientAsync(refusedPacket, connectionId);
                    _logger.LogWarning("Connection refused for client {Name} (ConnectionId: {ConnectionId}): {Reason}",
                        connectPacket.Name ?? "Unknown", connectionId, validationResult.Error);

                    return;
                }

                // If validation passed, attempt to map connectionId -> slot id (if a matching slot exists)
                int? matchedSlotId = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(connectPacket.Name) && _multiData.SlotInfo != null)
                    {
                        var match = _multiData.SlotInfo.FirstOrDefault(kvp =>
                            string.Equals(kvp.Value.Name, connectPacket.Name, StringComparison.OrdinalIgnoreCase) &&
                            (string.IsNullOrWhiteSpace(connectPacket.Game) || string.Equals(kvp.Value.Game, connectPacket.Game, StringComparison.OrdinalIgnoreCase)));

                        if (!match.Equals(default(KeyValuePair<int, Pickle.MultiDataNetworkSlot>)))
                        {
                            matchedSlotId = match.Key;
                        }
                    }

                    if (matchedSlotId.HasValue)
                    {
                        await _connectionManager.MapConnectionToSlotAsync(connectionId, matchedSlotId.Value);
                        _logger.LogInformation("Mapped connection {ConnectionId} to slot {Slot}", connectionId, matchedSlotId.Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error mapping connection to slot; continuing without mapping.");
                }
                
                // Build player list from MultiData
                NetworkPlayer[] players;
                try
                {
                    players = _multiData.SlotInfo.Select(kvp => new NetworkPlayer(0, (int)kvp.Key, kvp.Value.Name, kvp.Value.Name)).ToArray();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to build player list for Connected packet; sending empty list.");
                    players = Array.Empty<NetworkPlayer>();
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
                        _logger.LogDebug(ex, "Error retrieving hint points for slot {Slot}; defaulting to 0.", matchedSlotId.Value);
                        hintPoints = 0;
                    }
                }

                // SlotData: attempt to use per-slot data from MultiData.SlotData if we can identify the connecting slot
                Dictionary<string, object> slotDataForClient = new();
                try
                {
                    if (matchedSlotId.HasValue && _multiData.SlotData != null)
                    {
                        if (_multiData.SlotData.TryGetValue(matchedSlotId.Value, out var sd))
                        {
                            slotDataForClient = new Dictionary<string, object>(sd);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error extracting SlotData for client; falling back to empty SlotData.");
                    slotDataForClient = new Dictionary<string, object>();
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
                    _logger.LogDebug(ex, "Error loading recorded checks for slot; sending empty checks list.");
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
                var connectedPacket = new Connected(0, (long)(matchedSlotId ?? 0), players, missingChecks, locationsChecked, slotDataForClient, slotInfoDict, hintPoints);

                // Send Connected only to the connecting client
                await SendPacketToClientAsync(connectedPacket, connectionId);
                _logger.LogInformation("Sent Connected packet for client {Name} (ConnectionId: {ConnectionId})", connectPacket.Name ?? "Unknown", connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error handling Connect packet");
            }
        }

        /// <summary>
        /// Validation result for Connect packet validation.
        /// </summary>
        private record ValidationResult(bool IsValid, string? Error);

        /// <summary>
        /// Validates a Connect packet against server state and configuration.
        /// </summary>
        private ValidationResult ValidateConnectPacket(Connect packet)
        {
            try
            {
                // If Game is empty or whitespace, it's a Text Client — accept if a name is provided.
                if (string.IsNullOrWhiteSpace(packet.Game) || packet.Game == "Archipelago" )
                {
                    _logger.LogDebug("Empty game indicates a Text Client; skipping game/version checks.");

                    if (string.IsNullOrWhiteSpace(packet.Name))
                    {
                        _logger.LogDebug("Invalid or missing client name for Text Client");
                        return new ValidationResult(false, "IncompatibleVersion");
                    }

                    return new ValidationResult(true, null);
                }

                // Validate that the client name is specified
                if (string.IsNullOrWhiteSpace(packet.Name))
                {
                    _logger.LogDebug("Invalid or missing client name");
                    return new ValidationResult(false, "IncompatibleVersion");
                }

                // Validate version compatibility (if needed)
                if (packet.Version == null)
                {
                    _logger.LogDebug("Invalid or missing version");
                    return new ValidationResult(false, "IncompatibleVersion");
                }

                // Verify the game exists in our slot configuration
                var gameExists = _multiData.SlotInfo.Values.Any(slot => 
                    string.Equals(slot.Game, packet.Game, StringComparison.OrdinalIgnoreCase));

                if (!gameExists)
                {
                    _logger.LogDebug("Game not found in server configuration: {Game}", packet.Game);
                    return new ValidationResult(false, "IncompatibleVersion");
                }

                return new ValidationResult(true, null);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error during packet validation");
                return new ValidationResult(false, "IncompatibleVersion");
            }
        }

        /// <summary>
        /// Sends a packet to a specific client identified by connectionId.
        /// </summary>
        private async Task SendPacketToClientAsync(Packet packet, string connectionId)
        {
            try
            {
                await _connectionManager.SendJsonToConnectionAsync(connectionId, new[] { packet });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending packet to client {ConnectionId}", connectionId);
            }
        }
    }
}
