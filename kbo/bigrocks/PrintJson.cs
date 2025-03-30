namespace kbo.bigrocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type", IgnoreUnrecognizedTypeDiscriminators = true)]
[JsonDerivedType(typeof(ItemSendPrintJson), "ItemSend")]
[JsonDerivedType(typeof(ItemCheatPrintJson), "ItemCheat")]
[JsonDerivedType(typeof(HintPrintJson), "Hint")]
[JsonDerivedType(typeof(JoinPrintJson), "Join")]
[JsonDerivedType(typeof(PartPrintJson), "Part")]
[JsonDerivedType(typeof(ChatPrintJson), "Chat")]
[JsonDerivedType(typeof(TagsChangedPrintJson), "TagsChanged")]
[JsonDerivedType(typeof(GoalPrintJson), "Goal")]
[JsonDerivedType(typeof(ReleasePrintJson), "Release")]
[JsonDerivedType(typeof(CollectPrintJson), "Collect")]
[JsonDerivedType(typeof(ServerChatPrintJson), "ServerChat")]
[JsonDerivedType(typeof(CountdownPrintJson), "Countdown")]
[JsonDerivedType(typeof(TutorialPrintJson), "Tutorial")]
[JsonDerivedType(typeof(CommandResultPrintJson), "CommandResult")]
[JsonDerivedType(typeof(AdminCommandResultPrintJson), "AdminCommandResult")]
public record PrintJson : Packet
{
    /// <summary>
    /// Textual content of this message.
    /// </summary>
    [JsonPropertyName("data")]
    public JsonMessagePart.Text[] Data { get; set; }

    public PrintJson(JsonMessagePart.Text[] data)
    {
        Data = data;
    }
}

public record ItemPrintJson : PrintJson
{
    /// <summary>
    /// Destination player's ID.
    /// </summary>
    [JsonPropertyName("receiving")]
    public long Receiving { get; set; }

    /// <summary>
    /// Source player's ID, location ID, item ID and item flags.
    /// </summary>
    [JsonPropertyName("item")]
    public NetworkItem Item { get; set; }

    public ItemPrintJson(JsonMessagePart.Text[] data, long receiving, NetworkItem item) : base(data)
    {
        Receiving = receiving;
        Item = item;
    }
}

public record ItemCheatPrintJson : ItemPrintJson
{
    /// <summary>
    /// Team of the triggering player.
    /// </summary>
    [JsonPropertyName("team")]
    public long Team { get; set; }

    public ItemCheatPrintJson(JsonMessagePart.Text[] data, long receiving, NetworkItem item, long team) : base(data, receiving, item)
    {
        Team = team;
    }
}

public record ItemSendPrintJson : ItemPrintJson
{
    public ItemSendPrintJson(JsonMessagePart.Text[] data, long receiving, NetworkItem item) : base(data, receiving, item)
    {

    }
}

public record HintPrintJson : ItemPrintJson
{
    /// <summary>
    /// Whether the location hinted for was checked.
    /// </summary>
    [JsonPropertyName("found")]
    public bool Found { get; set; }

    public HintPrintJson(JsonMessagePart.Text[] data, long receiving, NetworkItem item, bool found) : base(data, receiving, item)
    {
        Found = found;
    }
}

public record SlotPrintJson : PrintJson
{
    /// <summary>
    /// Team of the triggering player.
    /// </summary>
    [JsonPropertyName("team")]
    public long Team { get; set; }

    /// <summary>
    /// Slot of the triggering player.
    /// </summary>
    [JsonPropertyName("slot")]
    public long Slot { get; set; }

    public SlotPrintJson(JsonMessagePart.Text[] data, long team, long slot) : base(data)
    {
        Team = team;
        Slot = slot;
    }
}

public record PartPrintJson : SlotPrintJson
{
    public PartPrintJson(JsonMessagePart.Text[] data, long slot, long team) : base(data, slot, team)
    {
    }
}

public record TagsPrintJson : SlotPrintJson
{
    /// <summary>
    /// Tags of the triggering player.
    /// </summary>
    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }
    public TagsPrintJson(JsonMessagePart.Text[] data, long slot, long team, string[] tags) : base(data, slot, team)
    {
        Tags = tags;
    }
}

public record JoinPrintJson : TagsPrintJson
{
    public JoinPrintJson(JsonMessagePart.Text[] data, long slot, long team, string[] tags) : base(data, slot, team, tags)
    {
    }
}

public record TagsChangedPrintJson : TagsPrintJson
{
    public TagsChangedPrintJson(JsonMessagePart.Text[] data, long slot, long team, string[] tags) : base(data, slot, team, tags)
    {
    }
}

public record GoalPrintJson : SlotPrintJson
{
    public GoalPrintJson(JsonMessagePart.Text[] data, long slot, long team) : base(data, slot, team)
    {
    }
}

public record ReleasePrintJson : SlotPrintJson
{
    public ReleasePrintJson(JsonMessagePart.Text[] data, long slot, long team) : base(data, slot, team)
    {
    }
}

public record CollectPrintJson : SlotPrintJson
{
    public CollectPrintJson(JsonMessagePart.Text[] data, long slot, long team) : base(data, slot, team)
    {
    }
}

public record ChatPrintJson : SlotPrintJson
{
    /// <summary>
    /// Original chat message without sender prefix.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    public ChatPrintJson(JsonMessagePart.Text[] data, long slot, long team, string message) : base(data, slot, team)
    {
        Message = message;
    }
}

public record ServerChatPrintJson : PrintJson
{
    /// <summary>
    /// Original chat message without sender prefix.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    public ServerChatPrintJson(JsonMessagePart.Text[] data, string message) : base(data)
    {
        Message = message;
    }
}

public record CountdownPrintJson : PrintJson
{
    /// <summary>
    /// Amount of seconds remaining on the countdown.
    /// </summary>
    [JsonPropertyName("countdown")]
    public long Countdown { get; set; }

    public CountdownPrintJson(JsonMessagePart.Text[] data, long countdown) : base(data)
    {
        Countdown = countdown;
    }
}

public record TutorialPrintJson : PrintJson
{
    public TutorialPrintJson(JsonMessagePart.Text[] data) : base(data)
    {
    }
}

public record CommandResultPrintJson : PrintJson
{
    public CommandResultPrintJson(JsonMessagePart.Text[] data) : base(data)
    {
    }
}

public record AdminCommandResultPrintJson : PrintJson
{
    public AdminCommandResultPrintJson(JsonMessagePart.Text[] data) : base(data)
    {
    }
}