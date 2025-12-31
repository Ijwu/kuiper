using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Plugins;
using kuiper.Services.Abstract;

namespace kuiper.Services
{
    /// <summary>
    /// Handles WebSocket connection lifecycle and message processing for Archipelago clients.
    /// </summary>
    public class WebSocketHandler : IWebSocketHandler
    {
        private readonly ILogger<WebSocketHandler> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly MultiData _multiData;
        private readonly PluginManager _pluginManager;

        public WebSocketHandler(
            ILogger<WebSocketHandler> logger,
            WebSocketConnectionManager connectionManager,
            MultiData multiData,
            PluginManager pluginManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
        }

        public async Task HandleConnectionAsync(string connectionId, PlayerData player)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                await SendRoomInfoAsync(player.Socket);

                var result = await player.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var messageBuilder = new StringBuilder();

                while (!result.CloseStatus.HasValue)
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageChunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        
                        if (messageBuilder.Length == 0)
                        {
                            _logger.LogDebug("Begin receiving: {MessageJson}", messageChunk);
                        }
                        else
                        {
                            _logger.LogDebug("Continuing receiving: {MessageJson}", messageChunk);
                        }

                        messageBuilder.Append(messageChunk);

                        if (result.EndOfMessage)
                        {
                            await ProcessMessageAsync(messageBuilder.ToString(), connectionId);
                            messageBuilder.Clear();
                        }
                    }

                    result = await player.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                _logger.LogDebug("Connection was closed. Connection Id: {ConnectionId}", connectionId);
                await _connectionManager.RemoveConnectionAsync(connectionId);
                await player.Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling WebSocket connection {ConnectionId}", connectionId);
                await _connectionManager.RemoveConnectionAsync(connectionId);
            }
        }

        private async Task SendRoomInfoAsync(WebSocket webSocket)
        {
            var roomInfo = new RoomInfo(new Version(0, 0, 1), // TODO: set correct protocol version
                                        _multiData.Version,
                                        _multiData.Tags,
                                        false, // TODO: Implement password handling
                                        GetPermissions(),
                                        _multiData.ServerOptions.TryGetValue("hint_cost", out var hc) ? Convert.ToInt32(hc) : 0,
                                        _multiData.ServerOptions.TryGetValue("location_check_points", out var lcp) ? Convert.ToInt32(lcp) : 0,
                                        _multiData.SlotInfo.Select(x => x.Value.Game).Distinct().ToArray(),
                                        _multiData.DataPackage.ToDictionary(x => x.Key, x => x.Value.Checksum),
                                        _multiData.SeedName,
                                        0d // TODO: Implement server time tracking
                                    );

            var json = JsonSerializer.Serialize(new List<Packet> { roomInfo });
            var bytes = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task ProcessMessageAsync(string json, string connectionId)
        {
            try
            {
                var settings = new JsonSerializerOptions();
                settings.AllowOutOfOrderMetadataProperties = true;

                var packets = JsonSerializer.Deserialize<List<Packet>>(json, settings);
                if (packets != null)
                {
                    foreach (var packet in packets)
                    {
                        await _pluginManager.BroadcastPacketAsync(packet, connectionId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing or broadcasting packet");
            }
        }

        private Dictionary<string, CommandPermission> GetPermissions()
        {
            var permissionsDict = new Dictionary<string, CommandPermission>
            {
                ["release"] = ParsePermission(_multiData.ServerOptions.GetValueOrDefault("release_mode")?.ToString()),
                ["remaining"] = ParsePermission(_multiData.ServerOptions.GetValueOrDefault("remaining_mode")?.ToString()),
                ["collect"] = ParsePermission(_multiData.ServerOptions.GetValueOrDefault("collect_mode")?.ToString())
            };

            return permissionsDict;
        }

        private static CommandPermission ParsePermission(string? value) => value switch
        {
            "enabled" => CommandPermission.Enabled,
            "disabled" => CommandPermission.Disabled,
            "auto" => CommandPermission.Auto,
            "auto-enabled" => CommandPermission.AutoEnabled,
            "goal" => CommandPermission.Goal,
            _ => CommandPermission.Disabled
        };
    }
}
