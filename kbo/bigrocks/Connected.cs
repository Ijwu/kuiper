namespace kbo.bigrocks;

public record Connected : Packet
{
    [JsonPropertyName("team")]
    public int Team { get; set; }

    [JsonPropertyName("slot")]
    public int Slot { get; set; }

    [JsonPropertyName("players")]
    public NetworkPlayer[] Players { get; set; }

    [JsonPropertyName("missing_locations")]
    public int[] MissingLocations { get; set; }

    [JsonPropertyName("checked_locations")]
    public int[] CheckedLocations { get; set; }

    [JsonPropertyName("slot_data")]
    public Dictionary<string, object>? SlotData { get; set; }

    [JsonPropertyName("slot_info")]
    public Dictionary<int, NetworkSlot> SlotInfo { get; set; }

    [JsonPropertyName("hint_points")]
    public int HintPoints { get; set; }

    public Connected(int team, int slot, NetworkPlayer[] players, int[] missingLocations,
                     int[] checkedLocations, Dictionary<string, object>? slotData,
                     Dictionary<int, NetworkSlot> slotInfo, int hintPoints)
    {
        Team = team;
        Slot = slot;
        Players = players;
        MissingLocations = missingLocations;
        CheckedLocations = checkedLocations;
        SlotData = slotData;
        SlotInfo = slotInfo;
        HintPoints = hintPoints;
    }
}
