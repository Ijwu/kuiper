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
[JsonDerivedType(typeof(Bounced), nameof(Bounced))]
[JsonDerivedType(typeof(InvalidPacket), nameof(InvalidPacket))]
public abstract record Packet
{
}
