using kbo.plantesimals;

namespace kbo.bigrocks;

public record RoomInfo : Packet
{
    [JsonPropertyName("version")]
    [JsonConverter(typeof(NetworkVersionConverter))]
    public Version Version { get; set; }

    [JsonPropertyName("generator_version")]
    [JsonConverter(typeof(NetworkVersionConverter))]        
    public Version GeneratorVersion { get; set; }

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }

    [JsonPropertyName("password")]
    public bool HasPassword { get; set; }

    [JsonPropertyName("permissions")]
    public Dictionary<string, CommandPermission> Permissions { get; set; }

    [JsonPropertyName("hint_cost")]
    public long HintCost { get; set; }

    [JsonPropertyName("location_check_points")]
    public long LocationCheckPoints { get; set; }

    [JsonPropertyName("games")]
    public string[] Games { get; set; }

    [JsonPropertyName("datapackage_versions")]
    public Dictionary<string, long> DataPackageVersions { get; set; }

    [JsonPropertyName("datapackage_checksums")]
    public Dictionary<string, string> DataPackageChecksums { get; set; }

    [JsonPropertyName("seed_name")]
    public string SeedName { get; set; }

    [JsonPropertyName("time")]
    public double Time { get; set; }

    public RoomInfo(Version version, Version generatorVersion, string[] tags, bool hasPassword,
                    Dictionary<string, CommandPermission> permissions, long hintCost, long locationCheckPoints,
                    string[] games, Dictionary<string, long> dataPackageVersions,
                    Dictionary<string, string> dataPackageChecksums, string seedName,
                    double time)
    {
        Version = version;
        GeneratorVersion = generatorVersion;
        Tags = tags;
        HasPassword = hasPassword;
        Permissions = permissions;
        HintCost = hintCost;
        LocationCheckPoints = locationCheckPoints;
        Games = games;
        DataPackageVersions = dataPackageVersions;
        DataPackageChecksums = dataPackageChecksums;
        SeedName = seedName;
        Time = time;
    }
}