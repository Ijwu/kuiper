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
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { AddNestedDerivedTypes }
            }
        };

        /// <remarks>
        /// Adapted from https://stackoverflow.com/questions/74604551/how-can-i-serialize-a-multi-level-polymorphic-type-hierarchy-with-system-text-js/74605703#74605703
        /// </remarks>
        static void AddNestedDerivedTypes(JsonTypeInfo jsonTypeInfo)
        {
            if (jsonTypeInfo.PolymorphismOptions is null) return;

            var derivedTypes = jsonTypeInfo.PolymorphismOptions.DerivedTypes
                .Where(t => Attribute.IsDefined(t.DerivedType, typeof(JsonDerivedTypeAttribute)))
                .Select(t => t.DerivedType)
                .ToList();
            var hashset = new HashSet<Type>(derivedTypes);
            var queue = new Queue<Type>(derivedTypes);
            while (queue.TryDequeue(out var derived))
            {
                if (!hashset.Contains(derived))
                {
                    // Todo: handle discriminators
                    jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derived, derived.Name.Replace("PrintJson", "")));
                    hashset.Add(derived);
                }

                var attribute = derived.GetCustomAttributes<JsonDerivedTypeAttribute>();
                foreach (var jsonDerivedTypeAttribute in attribute)
                {
                    queue.Enqueue(jsonDerivedTypeAttribute.DerivedType);
                }
            }
        }
    }
}
