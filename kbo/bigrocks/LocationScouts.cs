namespace kbo.bigrocks;

public record LocationScouts : Packet
{
    [JsonPropertyName("locations")]
    public long[] Locations { get; set; }

    [JsonPropertyName("create_as_hint")]
    public int CreateAsHint { get; set; }

    public LocationScouts(long[] locations, int createAsHint)
    {
        Locations = locations;
        CreateAsHint = createAsHint;
    }
}
