using System.Text.Json.Nodes;

namespace kbo.bigrocks;

public record SetReply : Packet
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("value")]
    public JsonNode Value { get; set; }

    [JsonPropertyName("original_value")]
    public JsonNode OriginalValue { get; set; }

    [JsonPropertyName("slot")]
    public long Slot { get; set; }

    public SetReply(string key, JsonNode value, JsonNode originalValue, long slot) : base("SetReply")
    {
        Key = key;
        Value = value;
        OriginalValue = originalValue;
        Slot = slot;
    }
}
