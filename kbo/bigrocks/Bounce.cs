using System.Text.Json.Nodes;

namespace kbo.bigrocks;

public record Bounce : Packet
{
     [JsonPropertyName("games")]
    public string[] Games { get; set; }

    [JsonPropertyName("slots")]
    public long[] Slots { get; set; }

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, JsonNode> Data { get; set; }

    public Bounce(string[] games, long[] slots, string[] tags, Dictionary<string, JsonNode> data)
    {
        Games = games;
        Slots = slots;
        Tags = tags;
        Data = data;
    }
}
