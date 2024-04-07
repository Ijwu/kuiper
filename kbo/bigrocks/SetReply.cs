using System.Text.Json.Nodes;

namespace kbo.bigrocks;

public record SetReply : Packet
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("value")]
    public JsonNode Value { get; set; }

    [JsonPropertyName("original_value")]
    public JsonNode? OriginalValue { get; set; }

    public SetReply(string key, JsonNode value, JsonNode? originalValue = null)
    {
        Key = key;
        Value = value;
        OriginalValue = originalValue;
    }
}
