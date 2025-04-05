namespace kbo.bigrocks;

public record StatusUpdate : Packet
{
    [JsonPropertyName("status")]
    public ClientStatus Status { get; set; }

    public StatusUpdate(ClientStatus status) : base("StatusUpdate")
    {
        Status = status;
    }
}
