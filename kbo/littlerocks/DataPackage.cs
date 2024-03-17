namespace kbo.littlerocks;

public record DataPackageGameData
{
    [JsonPropertyName("item_name_to_id")]
    public Dictionary<string, long> ItemNameToId { get; set; }

    [JsonPropertyName("location_name_to_id")]
    public Dictionary<string, long> LocationNameToId { get; set; }

    [JsonPropertyName("version")]
    public int? Version { get; set; }

    [JsonPropertyName("checksum")]
    public string Checksum { get; set; }

    public DataPackageGameData(Dictionary<string, long> itemNameToId, Dictionary<string, long> locationNameToId, string checksum, int? version = null)
    {
        ItemNameToId = itemNameToId;
        LocationNameToId = locationNameToId;
        Checksum = checksum;
        Version = Version;
    }
}
