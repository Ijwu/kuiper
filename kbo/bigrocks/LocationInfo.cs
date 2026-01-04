namespace kbo.bigrocks;

public record LocationInfo : Packet
{
    [JsonPropertyName("locations")]
    public NetworkObject[] Locations { get; set; }

    public LocationInfo(NetworkObject[] locations) : base("LocationInfo")
    {
        Locations = locations;
    }
}
