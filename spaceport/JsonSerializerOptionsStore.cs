using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
