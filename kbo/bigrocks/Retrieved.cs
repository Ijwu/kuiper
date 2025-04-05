using System.Text.Json.Nodes;

namespace kbo.bigrocks;

public record Retrieved : Packet
{
    [JsonPropertyName("keys")]
    public Dictionary<string, JsonNode> Keys { get; set; }

    public Retrieved(Dictionary<string, JsonNode> keys) : base("Retrieved")
    {
        Keys = keys;
    }
}
