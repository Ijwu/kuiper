using System.Text.Json.Serialization.Metadata;

namespace kbo.plantesimals;

public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        Type baseMessagePartType = typeof(TextJsonMessagePart);
        if (jsonTypeInfo.Type == baseMessagePartType)
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(PlayerJsonMessagePart), "location_id"),
                    new JsonDerivedType(typeof(PlayerAndFlagsJsonMessagePart), "item_id"),
                    new JsonDerivedType(typeof(ColorJsonMessagePart), "color")
                    
                }
            };
        }

        return jsonTypeInfo;
    }
}