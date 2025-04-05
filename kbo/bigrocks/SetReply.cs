namespace kbo.bigrocks;

public record SetReply : Packet
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("value")]
    public object Value { get; set; }

    [JsonPropertyName("original_value")]
    public object OriginalValue { get; set; }

    [JsonPropertyName("slot")]
    public long Slot { get; set; }

    public SetReply(string key, object value, object originalValue, long slot)
    {
        Key = key;
        Value = value;
        OriginalValue = originalValue;
        Slot = slot;
    }
}
