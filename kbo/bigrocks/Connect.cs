public record Connect : Packet
{
    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("game")]
    public string Game { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("uuid")]
    public Guid Uuid { get; set; }

    [JsonPropertyName("version")]
    public Version Version { get; set; }

    [JsonPropertyName("items_handling")]
    public int ItemsHandling { get; set; }

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }

    [JsonPropertyName("slot_data")]
    public bool SlotData { get; set; }

    public Connect(string password, string game, string name, Guid uuid,
                   Version version, int itemsHandling, string[] tags, bool slotData) : base("Connect")
    {
        Password = password;
        Game = game;
        Name = name;
        Uuid = uuid;
        Version = version;
        ItemsHandling = itemsHandling;
        Tags = tags;
        SlotData = slotData;
    }

// Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618
    internal Connect() : base("Connect")
    {
    }
#pragma warning restore CS8618
}