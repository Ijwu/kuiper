namespace kbo.bigrocks;

public record Connected : Packet
{
    [JsonPropertyName("team")]
    public long Team { get; set; }

    [JsonPropertyName("slot")]
    public long Slot { get; set; }

    [JsonPropertyName("players")]
    public NetworkPlayer[] Players { get; set; }

    [JsonPropertyName("missing_locations")]
    public long[] MissingLocations { get; set; }

    [JsonPropertyName("checked_locations")]
    public long[] CheckedLocations { get; set; }

    [JsonPropertyName("slot_data")]
    public Dictionary<string, object>? SlotData { get; set; }

    [JsonPropertyName("slot_info")]
    public Dictionary<long, NetworkSlot> SlotInfo { get; set; }

    [JsonPropertyName("hint_points")]
    public long HintPoints { get; set; }

    public Connected(long team, long slot, NetworkPlayer[] players, long[] missingLocations,
                     long[] checkedLocations, Dictionary<string, object>? slotData,
                     Dictionary<long, NetworkSlot> slotInfo, long hintPoints)
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
