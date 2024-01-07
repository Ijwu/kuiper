namespace kbo.littlerocks;

[Flags]
public enum NetworkItemFlags
{
    None = 0b000,
    Advancement = 0b001,
    Useful = 0b010,
    Trap = 0b100,
}
