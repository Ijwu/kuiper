namespace kbo;

public record SetNotify : Packet
{
    [JsonPropertyName("keys")]
    public string[] Keys { get; set; }

    public SetNotify(string[] keys)
    {
        Keys = keys;
    }
}
