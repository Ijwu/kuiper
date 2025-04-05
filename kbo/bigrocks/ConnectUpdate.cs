namespace kbo.bigrocks;

public record ConnectUpdate : Packet
{
    [JsonPropertyName("items_handling")]
    public ItemHandlingFlags ItemsHandling { get; set; }

    [JsonPropertyName("tags")]      
    public string[] Tags { get; set; }

    public ConnectUpdate(ItemHandlingFlags itemsHandling, string[] tags) : base("ConnectUpdate")
    {
        ItemsHandling = itemsHandling;
        Tags = tags;
    }
}
