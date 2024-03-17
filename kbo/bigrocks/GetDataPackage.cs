namespace kbo.bigrocks;

public record class GetDataPackage : Packet
{
    [JsonPropertyName("games")]
    public string[]? Games { get; set; }

    public GetDataPackage(string[]? games = null)
    {
        Games = games;
    }
}
