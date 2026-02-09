using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Services
{
    public class ServerAnnouncementService : IServerAnnouncementService
    {
        private readonly IConnectionManager _connectionManager;
        private readonly MultiData _multiData;
        private readonly ILogger<ServerAnnouncementService> _logger;

        public ServerAnnouncementService(
            IConnectionManager connectionManager,
            MultiData multiData,
            ILogger<ServerAnnouncementService> logger)
        {
            _connectionManager = connectionManager;
            _multiData = multiData;
            _logger = logger;
        }

        public async Task AnnouncePlayerConnectedAsync(long slotId, string playerName)
        {
            _logger.LogInformation("Server: Player {PlayerName} (slot {SlotId}) connected", playerName, slotId);

            var packet = CreatePrintJsonPacket(
                Player(slotId),
                Text(" has connected.")
            );

            await BroadcastPacketAsync(packet);
        }

        public async Task AnnouncePlayerDisconnectedAsync(long slotId, string playerName)
        {
            _logger.LogInformation("Server: Player {PlayerName} (slot {SlotId}) disconnected", playerName, slotId);

            var packet = CreatePrintJsonPacket(
                Player(slotId),
                Text(" has disconnected.")
            );

            await BroadcastPacketAsync(packet);
        }

        public async Task AnnounceGoalReachedAsync(long slotId, string playerName)
        {
            _logger.LogInformation("Server: Player {PlayerName} (slot {SlotId}) reached their goal", playerName, slotId);

            var packet = CreatePrintJsonPacket(
                Player(slotId),
                Text(" has reached their goal!")
            );

            await BroadcastPacketAsync(packet);
        }

        public async Task AnnounceItemSentAsync(long senderSlotId, long receiverSlotId, string itemName, long itemId, long locationId)
        {
            var senderName = GetPlayerName((int)senderSlotId);
            var receiverName = GetPlayerName((int)receiverSlotId);

            _logger.LogInformation("Server: Player {SenderName} sent {ItemName} to {ReceiverName}",
                senderName, itemName, receiverName);

            var packet = CreatePrintJsonPacket(
                Player(senderSlotId),
                Text(" sent "),
                Item(itemId, receiverSlotId),
                Text(" to "),
                Player(receiverSlotId),
                Text(" ("),
                Location(locationId, senderSlotId),
                Text(")")
            );

            await BroadcastPacketAsync(packet);
        }

        public async Task BroadcastChatMessageAsync(long senderSlotId, string message)
        {
            var senderName = GetPlayerName((int)senderSlotId);
            _logger.LogInformation("[Chat] {PlayerName}: {Message}", senderName, message);

            var packet = CreatePrintJsonPacket(
                Player(senderSlotId),
                Text(": "),
                Text(message)
            );

            await BroadcastPacketAsync(packet);
        }

        public async Task BroadcastServerMessageAsync(string message)
        {
            _logger.LogInformation("Server: {Message}", message);

            var packet = CreatePrintJsonPacket(
                Text("[Server] "),
                Text(message)
            );

            await BroadcastPacketAsync(packet);
        }

        public async Task AnnounceHintAsync(long receivingSlotId, long findingSlotId, long itemId, long locationId, NetworkItemFlags flags)
        {
            _logger.LogInformation("Server: Hint revealed: Player {Receiver} item {Item} at {Location} in {Finder}",
                receivingSlotId, itemId, locationId, findingSlotId);

            var packet = CreatePrintJsonPacket(
                Text("[Hint]: "),
                Player(receivingSlotId),
                Text("'s "),
                Item(itemId, receivingSlotId, flags),
                Text(" is at "),
                Location(locationId, findingSlotId),
                Text(" in "),
                Player(findingSlotId),
                Text("'s World")
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

        private static JsonMessagePart.Text Text(string text)
        {
            return new JsonMessagePart.Text(text);
        }

        private static JsonMessagePart.Text Player(long slotId)
        {
            return new JsonMessagePart.PlayerId(slotId.ToString());
        }

        private static JsonMessagePart.Text Item(long itemId, long player)
        {
            return Item(itemId, player, NetworkItemFlags.None);
        }

        private static JsonMessagePart.Text Item(long itemId, long player, NetworkItemFlags flags)
        {
            return new JsonMessagePart.ItemId(itemId.ToString(), flags, player);
        }

        private static JsonMessagePart.Text Location(long locationId, long player)
        {
            return new JsonMessagePart.LocationId(locationId.ToString(), player);
        }

        private async Task BroadcastPacketAsync(PrintJson packet)
        {
            await _connectionManager.BroadcastJsonAsync<Packet[]>([packet]);
        }
    }
}
