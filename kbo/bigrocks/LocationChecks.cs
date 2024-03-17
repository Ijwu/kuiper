namespace kbo.bigrocks;

public record LocationChecks : Packet
{
    [JsonPropertyName("locations")]
    public long[] Locations { get; set; }

    public LocationChecks(long[] locations)
    {
        Locations = locations;
    }
}
