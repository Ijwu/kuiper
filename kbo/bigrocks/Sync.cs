namespace kbo.bigrocks;

public record Sync : Packet
{
    public Sync() : base("Sync")
    {
        
    }
}
