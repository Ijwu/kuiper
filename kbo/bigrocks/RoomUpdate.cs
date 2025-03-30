namespace kbo.bigrocks;

public record RoomUpdate : Packet
{
    [JsonPropertyName("hint_points")]
    public long? HintPoints { get; set; }

    [JsonPropertyName("players")]
    public NetworkPlayer[]? Players { get; set; }

    [JsonPropertyName("checked_locations")]
    public long[]? CheckedLocations { get; set; }

    [JsonPropertyName("hint_cost")]
    public long? HintCost { get; set; }

    [JsonPropertyName("location_check_points")]
    public long? LocationCheckPoints { get; set; }

    [JsonPropertyName("permissions")]
    public Dictionary<string, CommandPermission>? Permissions { get; set; }

    public RoomUpdate(long? hintPoints,
                      NetworkPlayer[]? players,
                      long[]? checkedLocations,
                      long? hintCost,
                      long? locationCheckPoints,
                      Dictionary<string, CommandPermission>? permissions)
    {
        HintPoints = hintPoints;
        Players = players;
        CheckedLocations = checkedLocations;
        HintCost = hintCost;
        LocationCheckPoints = locationCheckPoints;
        Permissions = permissions;
    }
}

