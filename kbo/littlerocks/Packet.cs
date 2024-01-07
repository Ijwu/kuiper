using kbo.bigrocks;

namespace kbo.littlerocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "cmd")]
[JsonDerivedType(typeof(Connect), "Connect")]
[JsonDerivedType(typeof(RoomInfo), "RoomInfo")]
public abstract record Packet
{
}
