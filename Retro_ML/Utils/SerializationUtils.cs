using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Retro_ML.Utils
{
    internal static class SerializationUtils
    {
        public static readonly JsonSerializerSettings JSON_CONFIG = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        public static readonly JsonSerializerSettings JSON_PASCAL_CASE_CONFIG = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };
    }
}
