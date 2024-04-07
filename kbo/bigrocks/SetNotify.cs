namespace kbo.bigrocks;

public record SetNotify : Packet
{
    [JsonPropertyName("keys")]
    public string[] Keys { get; set; }

    public SetNotify(string[] keys)
    {
        Keys = keys;
    }
}
