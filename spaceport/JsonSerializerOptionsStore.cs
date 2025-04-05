using System.Text.Json;

namespace spaceport
{
    public static class JsonSerializerOptionsStore
    {
        public static JsonSerializerOptions Deserialization => new JsonSerializerOptions()
        {
            AllowOutOfOrderMetadataProperties = true,
        };
    }
}
