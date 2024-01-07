using kbo.bigrocks;

namespace kbo.plantesimals;

public class PacketStreamConverter : JsonConverter<Packet[]>
{
    public override Packet[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var packets = new List<Packet>();
        
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            Packet packet = JsonSerializer.Deserialize<Packet>(ref reader, options)!;
            packets.Add(packet);
        }

        return packets.Where(x => x is not null).ToArray();
    }

    public override void Write(Utf8JsonWriter writer, Packet[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (Packet packet in value)
        {
            JsonSerializer.Serialize(writer, packet, options);
        }
        writer.WriteEndArray();
    }
}