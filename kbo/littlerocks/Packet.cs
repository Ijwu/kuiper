using kbo.bigrocks;

namespace kbo.littlerocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "cmd")]
[JsonDerivedType(typeof(Connect), nameof(Connect))]
[JsonDerivedType(typeof(RoomInfo), nameof(RoomInfo))]
[JsonDerivedType(typeof(Connected), nameof(Connected))]
[JsonDerivedType(typeof(ConnectionRefused), nameof(ConnectionRefused))]
[JsonDerivedType(typeof(LocationInfo), nameof(LocationInfo))]
[JsonDerivedType(typeof(PrintJson), nameof(PrintJson))]
[JsonDerivedType(typeof(RoomUpdate), nameof(RoomUpdate))]
[JsonDerivedType(typeof(DataPackage), nameof(DataPackage))]
[JsonDerivedType(typeof(Bounced), nameof(Bounced))]
[JsonDerivedType(typeof(InvalidPacket), nameof(InvalidPacket))]
[JsonDerivedType(typeof(ConnectUpdate), nameof(ConnectUpdate))]
[JsonDerivedType(typeof(Sync), nameof(Sync))]
[JsonDerivedType(typeof(LocationChecks), nameof(LocationChecks))]
[JsonDerivedType(typeof(LocationScouts), nameof(LocationScouts))]
[JsonDerivedType(typeof(StatusUpdate), nameof(StatusUpdate))]
[JsonDerivedType(typeof(Say), nameof(Say))]
[JsonDerivedType(typeof(GetDataPackage), nameof(GetDataPackage))]
[JsonDerivedType(typeof(Bounce), nameof(Bounce))]
public abstract record Packet
{
}
