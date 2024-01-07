namespace kbo.bigrocks;

public record RoomUpdate : Packet
{
    [JsonPropertyName("players")]
    public NetworkPlayer[] Players { get; set; }

    [JsonPropertyName("checked_locations")]
    public long[] CheckedLocations { get; set; }

    public RoomUpdate(NetworkPlayer[] players, long[] checkedLocations)
    {
        Players = players;
        CheckedLocations = checkedLocations;
    }
}

