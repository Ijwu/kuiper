using kbo.bigrocks;

namespace kbo.plantesimals
{
    public class PacketConverter : JsonConverter<Packet>
    {
        private readonly Dictionary<string, Type> _typeMap = new()
        {
            { "RoomInfo", typeof(RoomInfo) },
            { "ConnectionRefused", typeof(ConnectionRefused) },
            { "Connected", typeof(Connected) },
            { "ReceivedItems", typeof(ReceivedItems) },
            { "LocationInfo", typeof(LocationInfo) },
            { "RoomUpdate", typeof(RoomUpdate) },
            { "PrintJSON", typeof(PrintJson) },
            { "DataPackage", typeof(DataPackage) },
            { "Bounced", typeof(Bounced) },
            { "InvalidPacket", typeof(InvalidPacket) },
            { "Retrieved", typeof(Retrieved) },
            { "SetReply", typeof(SetReply) },
            { "Connect", typeof(Connect) },
            { "ConnectUpdate", typeof(ConnectUpdate) },
            { "Sync", typeof(Sync) },
            { "LocationChecks", typeof(LocationChecks) },
            { "LocationScouts", typeof(LocationScouts) },
            { "UpdateHint", typeof(UpdateHint) },
            { "StatusUpdate", typeof(StatusUpdate) },
            { "Say", typeof(Say) },
            { "GetDataPackage", typeof(GetDataPackage) },
            { "Bounce", typeof(Bounce) },
            { "Get", typeof(Get) },
            { "Set", typeof(Set) },
            { "SetNotify", typeof(SetNotify) }
        };

        public override Packet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Parse the JSON document to inspect the discriminator.
            using (var document = JsonDocument.ParseValue(ref reader))
            {
                var root = document.RootElement;

                // Assume the outer discriminator is named "type"
                if (!root.TryGetProperty("cmd", out var typeProperty))
                {
                    throw new JsonException("Missing discriminator property 'cmd'.");
                }

                string? typeDiscriminator = typeProperty.GetString();
                Packet result;

                // Get type from typemap using disciriminator value
                if (_typeMap.TryGetValue(typeDiscriminator, out var type))
                {
                    // Deserialize the JSON to the specific type
                    result = (Packet)JsonSerializer.Deserialize(root.GetRawText(), type, options)!;
                }
                else
                {
                    throw new JsonException($"Unknown discriminator value '{typeDiscriminator}'.");
                }

                return result;
            }
        }

        public override void Write(Utf8JsonWriter writer, Packet value, JsonSerializerOptions options)
        {
            // Use runtime type to serialize the correct type.
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
