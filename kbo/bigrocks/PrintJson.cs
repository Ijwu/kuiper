namespace kbo.bigrocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "data", IgnoreUnrecognizedTypeDiscriminators = true)]
[JsonDerivedType(typeof(Chat), nameof(Chat))]
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
public partial record PrintJson : Packet
{
    [JsonPropertyName("data")]
    public BaseJsonMessagePart[] Data { get; set; }

    [JsonPropertyName("type")]
    public string PrintJsonType { get; set; }

    public PrintJson(BaseJsonMessagePart[] data, string printJsonType)
    {
        Data = data;
        PrintJsonType = printJsonType;
    }
}