using kbo.bigrocks;

namespace kbo.littlerocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "cmd")]
[JsonDerivedType(typeof(Connect), "Connect")]
[JsonDerivedType(typeof(RoomInfo), "RoomInfo")]
[JsonDerivedType(typeof(Connected), "Connected")]
public abstract record Packet
{
}
