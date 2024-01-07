namespace kbo.littlerocks;

[Flags]
public enum ItemHandlingFlags
{
    None = 0b000,
    Remote = 0b001,
    LocalAndRemote = 0b011,
    StartingInventoryAndRemote = 0b101,
    All = 0b111,
}
