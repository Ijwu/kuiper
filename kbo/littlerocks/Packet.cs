using kbo.bigrocks;

namespace kbo.littlerocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "cmd", IgnoreUnrecognizedTypeDiscriminators = true)]
// Server to Client
[JsonDerivedType(typeof(RoomInfo), nameof(RoomInfo))]
[JsonDerivedType(typeof(ConnectionRefused), nameof(ConnectionRefused))]
[JsonDerivedType(typeof(Connected), nameof(Connected))]
[JsonDerivedType(typeof(ReceivedItems), nameof(ReceivedItems))]
[JsonDerivedType(typeof(LocationInfo), nameof(LocationInfo))]
[JsonDerivedType(typeof(RoomUpdate), nameof(RoomUpdate))]
[JsonDerivedType(typeof(PrintJson), "PrintJSON")]
[JsonDerivedType(typeof(DataPackage), nameof(DataPackage))]
[JsonDerivedType(typeof(Bounced), nameof(Bounced))]
[JsonDerivedType(typeof(InvalidPacket), nameof(InvalidPacket))]
[JsonDerivedType(typeof(Retrieved), nameof(Retrieved))]
[JsonDerivedType(typeof(SetReply), nameof(SetReply))]

// Client to Server
[JsonDerivedType(typeof(Connect), nameof(Connect))]
[JsonDerivedType(typeof(ConnectUpdate), nameof(ConnectUpdate))]
[JsonDerivedType(typeof(Sync), nameof(Sync))]
[JsonDerivedType(typeof(LocationChecks), nameof(LocationChecks))]
[JsonDerivedType(typeof(LocationScouts), nameof(LocationScouts))]
[JsonDerivedType(typeof(StatusUpdate), nameof(StatusUpdate))]
[JsonDerivedType(typeof(Say), nameof(Say))]
[JsonDerivedType(typeof(GetDataPackage), nameof(GetDataPackage))]
[JsonDerivedType(typeof(Bounce), nameof(Bounce))]
[JsonDerivedType(typeof(Set), nameof(Set))]
[JsonDerivedType(typeof(Get), nameof(Get))]
[JsonDerivedType(typeof(SetNotify), nameof(SetNotify))]
public record Packet
{
}
