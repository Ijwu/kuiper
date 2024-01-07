namespace kbo.littlerocks;

public record NetworkSlot : NetworkObject
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("game")]
    public string Game { get; set; }

    [JsonPropertyName("type")]
    public SlotType SlotType { get; set; }

    [JsonPropertyName("group_members")]
    public long[] GroupMembers { get; set; }

    public NetworkSlot(string name, string game, SlotType slotType, long[] groupMembers)
    {
        Name = name;
        Game = game;
        SlotType = slotType;
        GroupMembers = groupMembers;
    }
}
