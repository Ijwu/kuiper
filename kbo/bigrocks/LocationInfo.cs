namespace kbo.bigrocks;

public record LocationInfo : Packet
{
    [JsonPropertyName("locations")]
    public NetworkItem[] Locations { get; set; }

    public LocationInfo(NetworkItem[] locations)
    {
        Locations = locations;
    }
}
