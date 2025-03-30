namespace kbo.bigrocks;

public record LocationScouts : Packet
{
    [JsonPropertyName("locations")]
    public long[] Locations { get; set; }

    [JsonPropertyName("create_as_hint")]
    public long CreateAsHint { get; set; }

    public LocationScouts(long[] locations, long createAsHint)
    {
        Locations = locations;
        CreateAsHint = createAsHint;
    }
}
