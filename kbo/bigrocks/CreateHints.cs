namespace kbo.bigrocks;

public record CreateHints : Packet
{
    [JsonPropertyName("locations")]
    public long[] Locations { get; set; }

    [JsonPropertyName("player")]
    public long Player { get; set; }

    [JsonPropertyName("status")]
    public HintStatus Status { get; set; } = HintStatus.Unspecified;

    public CreateHints(long[] locations) : base("CreateHints")
    {
        Locations = locations;
    }
}
