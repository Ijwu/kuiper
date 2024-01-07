using kbo.plantesimals;

namespace kbo.bigrocks;

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
    [JsonConverter(typeof(NetworkVersionConverter))]
    public Version Version { get; set; }

    [JsonPropertyName("items_handling")]
    public ItemHandlingFlags ItemsHandling { get; set; }

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }

    [JsonPropertyName("slot_data")]
    public bool SlotData { get; set; }

    public Connect(string password, string game, string name, Guid uuid,
                   Version version, ItemHandlingFlags itemsHandling, string[] tags, 
                   bool slotData)
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
}