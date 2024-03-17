namespace kbo.bigrocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type", IgnoreUnrecognizedTypeDiscriminators = true)]
[JsonDerivedType(typeof(ItemSend), nameof(ItemSend))]
[JsonDerivedType(typeof(ItemCheat), nameof(ItemCheat))]
[JsonDerivedType(typeof(Hint), nameof(Hint))]
[JsonDerivedType(typeof(Join), nameof(Join))]
[JsonDerivedType(typeof(Part), nameof(Part))]
[JsonDerivedType(typeof(Chat), nameof(Chat))]
[JsonDerivedType(typeof(ServerChat), nameof(ServerChat))]
[JsonDerivedType(typeof(TagsChanged), nameof(TagsChanged))]
[JsonDerivedType(typeof(Goal), nameof(Goal))]
[JsonDerivedType(typeof(Release), nameof(Release))]
[JsonDerivedType(typeof(Collect), nameof(Collect))]
[JsonDerivedType(typeof(Countdown), nameof(Countdown))]
[JsonDerivedType(typeof(CommandResult), nameof(CommandResult))]
[JsonDerivedType(typeof(AdminCommandResult), nameof(AdminCommandResult))]
public partial record PrintJson : Packet
{
    [JsonPropertyName("data")]
    public JsonMessagePart[] Data { get; set; }

    public PrintJson(JsonMessagePart[] data)
    {
        Data = data;
    }
}