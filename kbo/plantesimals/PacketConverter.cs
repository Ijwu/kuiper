namespace kbo.plantesimals;

public class PacketConverter : JsonConverter<Packet>
{
    public override Packet? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName)
            throw new JsonException();

        var packetType = reader.GetString();
        if (packetType == null)
            throw new JsonException();

        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        Packet? packet = packetType switch
        {
            "RoomInfo" => JsonSerializer.Deserialize<RoomInfo>(ref reader, options),
            "Connect" => JsonSerializer.Deserialize<Connect>(ref reader, options),
            _ => throw new JsonException($"Unknown packet type: {packetType}")
        };

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            throw new JsonException();

        return packet;
    }

    public override void Write(Utf8JsonWriter writer, Packet value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("cmd", value.PacketType);
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
        writer.WriteEndObject();
    }
}