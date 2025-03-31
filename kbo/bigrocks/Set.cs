using System.Text.Json.Nodes;

namespace kbo.bigrocks;

public record Set : Packet
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("default")]
    public object Default { get; set; }

    [JsonPropertyName("want_reply")]
    public bool WantReply { get; set; }

    [JsonPropertyName("operations")]
    public DataStorageOperation[] Operations { get; set; }

    public Set(string key, object @default, bool wantReply, DataStorageOperation[] operations)
    {
        Key = key;
        Default = @default;
        WantReply = wantReply;
        Operations = operations;
    }
}
