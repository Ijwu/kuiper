using System.Text.Json;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services.Abstract;

namespace kuiper.Services
{
    /// <summary>
    /// Service for broadcasting server announcements and messages to connected clients
    /// using properly formatted PrintJSON packets.
    /// </summary>
    public class ServerAnnouncementService : IServerAnnouncementService
    {
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly MultiData _multiData;
        private readonly ILogger<ServerAnnouncementService> _logger;

        public ServerAnnouncementService(
            WebSocketConnectionManager connectionManager,
            MultiData multiData,
            ILogger<ServerAnnouncementService> logger)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AnnouncePlayerConnectedAsync(long slotId, string playerName)
        {
            _logger.LogInformation("Player {PlayerName} (slot {SlotId}) connected", playerName, slotId);

            var packet = CreatePrintJsonPacket(
                CreatePlayerPart(slotId),
                CreateTextPart(" has connected.")
            );

            await BroadcastPacketAsync(packet);
        }

        public async Task AnnouncePlayerDisconnectedAsync(long slotId, string playerName)
        {
            _logger.LogInformation("Player {PlayerName} (slot {SlotId}) disconnected", playerName, slotId);

            var packet = CreatePrintJsonPacket(
                CreatePlayerPart(slotId),
                CreateTextPart(" has disconnected.")
            );

            await BroadcastPacketAsync(packet);
        }

        public async Task AnnounceGoalReachedAsync(long slotId, string playerName)
        {
            _logger.LogInformation("Player {PlayerName} (slot {SlotId}) reached their goal", playerName, slotId);

            var packet = CreatePrintJsonPacket(
                CreatePlayerPart(slotId),
                CreateTextPart(" has reached their goal!")
            );

            await BroadcastPacketAsync(packet);
        }

        public async Task AnnounceItemSentAsync(long senderSlotId, long receiverSlotId, string itemName, long itemId, long locationId)
        {
            var senderName = GetPlayerName((int)senderSlotId);
            var receiverName = GetPlayerName((int)receiverSlotId);

            _logger.LogInformation("Player {SenderName} sent {ItemName} to {ReceiverName}",
                senderName, itemName, receiverName);

            var packet = CreatePrintJsonPacket(
                CreatePlayerPart(senderSlotId),
                CreateTextPart(" sent "),
                CreateItemPart(itemId, receiverSlotId),
                CreateTextPart(" to "),
                CreatePlayerPart(receiverSlotId),
                CreateTextPart(" ("),
                CreateLocationPart(locationId, senderSlotId),
                CreateTextPart(")")
            );

            await BroadcastPacketAsync(packet);
        }

        public async Task BroadcastChatMessageAsync(long senderSlotId, string message)
        {
            var senderName = GetPlayerName((int)senderSlotId);
            _logger.LogInformation("[Chat] {PlayerName}: {Message}", senderName, message);

            var packet = CreatePrintJsonPacket(
                CreatePlayerPart(senderSlotId),
                CreateTextPart(": "),
                CreateTextPart(message)
            );

            await BroadcastPacketAsync(packet);
        }

        public async Task BroadcastServerMessageAsync(string message)
        {
            _logger.LogInformation("[Server] {Message}", message);

            var packet = CreatePrintJsonPacket(
                CreateTextPart("[Server] "),
                CreateTextPart(message)
            );

            await BroadcastPacketAsync(packet);
        }

        private string GetPlayerName(int slotId)
        {
            if (_multiData.SlotInfo.TryGetValue(slotId, out var slot))
            {
                return slot.Name;
            }
            return $"Player {slotId}";
        }

        private static PrintJson CreatePrintJsonPacket(params JsonMessagePart.Text[] parts)
        {
            return new PrintJson(parts);
        }

        private static JsonMessagePart.Text CreateTextPart(string text)
        {
            return new JsonMessagePart.Text(text);
        }

        private static JsonMessagePart.Text CreatePlayerPart(long slotId)
        {
            return new JsonMessagePart.PlayerId(slotId.ToString());
        }

        private static JsonMessagePart.Text CreateItemPart(long itemId, long player)
        {
            return new JsonMessagePart.ItemId(itemId.ToString(), NetworkItemFlags.None, player); //TODO: Set proper flags
        }

        private static JsonMessagePart.Text CreateLocationPart(long locationId, long player)
        {
            return new JsonMessagePart.LocationId(locationId.ToString(), player);
        }

        private async Task BroadcastPacketAsync(PrintJson packet)
        {
            var json = JsonSerializer.Serialize(new[] { packet });
            await _connectionManager.BroadcastAsync(json);
        }
    }
}
