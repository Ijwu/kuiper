using System.Text.Json;
using System.Text.Json.Serialization;

namespace kuiper.Core.Constants
{
    public static class Json
    {
        public static JsonSerializerOptions NetworkDefaultOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
