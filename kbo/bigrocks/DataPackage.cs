namespace kbo.bigrocks;

public record DataPackage : Packet
{
    [JsonPropertyName("games")]
    public Dictionary<string, DataPackageGameData> Games { get; set; }

    public DataPackage(Dictionary<string, DataPackageGameData> games)
    {
        Games = games;
    }
}
