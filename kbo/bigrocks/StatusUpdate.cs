namespace kbo.bigrocks;

public record class StatusUpdate : Packet
{
    [JsonPropertyName("status")]
    public ClientStatus Status { get; set; }

    public StatusUpdate(ClientStatus status)
    {
        Status = status;
    }
}
