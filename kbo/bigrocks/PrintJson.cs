namespace kbo.bigrocks;

public partial record PrintJson : Packet
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("data")]
    public JsonMessagePart.Text[] Data { get; set; }

    public PrintJson(string type, JsonMessagePart.Text[] data)
    {
        Type = type;
        Data = data;
    }
}