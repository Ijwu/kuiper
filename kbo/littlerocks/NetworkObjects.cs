namespace kbo.littlerocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "class")]
[JsonDerivedType(typeof(NetworkPlayer), "NetworkPlayer")]
[JsonDerivedType(typeof(NetworkSlot), "NetworkSlot")]
[JsonDerivedType(typeof(NetworkItem), "NetworkItem")]
public abstract record NetworkObject
{

}

public record NetworkPlayer : NetworkObject
{
    [JsonPropertyName("team")]
    public long Team { get; set; }

    [JsonPropertyName("slot")]
    public long Slot { get; set; }

    [JsonPropertyName("alias")]
    public string Alias { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    public NetworkPlayer(long team, long slot, string alias, string name)
    {
        Team = team;
        Slot = slot;
        Alias = alias;
        Name = name;
    }
}

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

public record NetworkItem : NetworkObject
{
    [JsonPropertyName("item")]
    public long Item { get; set; }

    [JsonPropertyName("location")]
    public long Location { get; set; }

    [JsonPropertyName("player")]
    public long Player { get; set; }

    [JsonPropertyName("flags")]
    public NetworkItemFlags Flags { get; set; }

    public NetworkItem(long item, long location, long player, NetworkItemFlags flags)
    {
        Item = item;
        Location = location;
        Player = player;
        Flags = flags;
    }
}