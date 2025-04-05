namespace kbo.bigrocks;

public record DataPackage : Packet
{
    [JsonPropertyName("data")]
    public DataPackageContents Data { get; set; }

    public DataPackage(DataPackageContents data) : base("DataPackage")
    {
        Data = data;
    }
}
