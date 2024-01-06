public record RoomInfo : Packet
{
    [JsonPropertyName("version")]
    public Version Version { get; set; }

    [JsonPropertyName("generator_version")]
    public Version GeneratorVersion { get; set; }

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }

    [JsonPropertyName("password")]
    public bool HasPassword { get; set; }

    [JsonPropertyName("permissions")]
    public Dictionary<string, int> Permissions { get; set; }

    [JsonPropertyName("hint_cost")]
    public int HintCost { get; set; }

    [JsonPropertyName("location_check_points")]
    public int LocationCheckPoints { get; set; }

    [JsonPropertyName("games")]
    public string[] Games { get; set; }

    [JsonPropertyName("datapackage_versions")]
    public Dictionary<string, int> DataPackageVersions { get; set; }

    [JsonPropertyName("datapackage_checksums")]
    public Dictionary<string, string> DataPackageChecksums { get; set; }

    [JsonPropertyName("seed_name")]
    public string SeedName { get; set; }

    [JsonPropertyName("time")]
    public long Time { get; set; }

    public RoomInfo(Version version, Version generatorVersion, string[] tags, bool hasPassword,
                    Dictionary<string,int> permissions, int hintCost, int locationCheckPoints,
                    string[] games, Dictionary<string, int> dataPackageVersions,
                    Dictionary<string, string> dataPackageChecksums, string seedName,
                    long time) : base("RoomInfo")
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

// Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618
    internal RoomInfo() : base("RoomInfo")
    {
        
    }
#pragma warning restore CS8618
}