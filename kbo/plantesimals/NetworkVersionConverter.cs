namespace kbo.plantesimals;

public class NetworkVersionConverter : JsonConverter<Version>
{
    public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        int major = 0;
        int minor = 0;
        int build = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "major":
                    major = reader.GetInt32();
                    break;
                case "minor":
                    minor = reader.GetInt32();
                    break;
                case "build":
                    build = reader.GetInt32();
                    break;
                case "class":
                    reader.Skip();
                    break;
            }
        }

        return new Version(major, minor, build);
    }

    public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("major", value.Major);
        writer.WriteNumber("minor", value.Minor);
        writer.WriteNumber("build", value.Build);
        writer.WriteEndObject();
    }
}