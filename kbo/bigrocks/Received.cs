using System.Text.Json.Nodes;

namespace kbo.bigrocks;

public record Received : Packet
{
    [JsonPropertyName("keys")]
    public Dictionary<string, JsonNode> Keys { get; set; }

    public Received(Dictionary<string, JsonNode> keys)
    {
        Keys = keys;
    }
}
