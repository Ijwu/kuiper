namespace kbo.bigrocks;

public record ConnectionRefused : Packet
{
    [JsonPropertyName("errors")]
    public string[] Errors { get; set; }

    public ConnectionRefused(string[] errors) : base("ConnectionRefused")
    {
        Errors = errors;
    }
}
