namespace kbo.littlerocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "class")]
[JsonDerivedType(typeof(NetworkPlayer), "NetworkPlayer")]
[JsonDerivedType(typeof(NetworkSlot), "NetworkSlot")]
[JsonDerivedType(typeof(NetworkItem), "NetworkItem")]
public abstract record NetworkObject
{

}