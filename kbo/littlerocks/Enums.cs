using System.Runtime.Serialization;

namespace kbo.littlerocks;

public enum ClientStatus
{
    Unknown = 0,
    Connected = 5,
    Ready = 10,
    Playing = 20,
    Goal = 30
}

public enum HintStatus
{
    Unspecified = 0,
    NoPriority = 10,
    Avoid = 20,
    Priority = 30,
    Found = 40
}

[Flags]
public enum NetworkItemFlags
{
    None = 0b000,
    Advancement = 0b001,
    Useful = 0b010,
    Trap = 0b100,
}

[Flags]
public enum ItemHandlingFlags
{
    None = 0b000,
    Remote = 0b001,
    LocalAndRemote = 0b011,
    StartingInventoryAndRemote = 0b101,
    All = 0b111,
}

[Flags]
public enum SlotType
{
    Spectator = 0b00,
    Player = 0b01,
    Group = 0b10,
}

public enum CommandPermission
{
    Disabled = 0,
    Enabled = 1,
    Goal = 2,
    Auto = 6,
    AutoEnabled = 7
}