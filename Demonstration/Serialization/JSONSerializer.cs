using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Demonstration.Serialization
{
    class JSONSerializer : ISerializer
    {
        public string Serizlize(object o)
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings() { ContractResolver = new AllFieldsContractResolver() };
            settings.Formatting = Formatting.Indented;
            return JsonConvert.SerializeObject(o, settings);// JsonSerializer.Serialize(o, o.GetType(), options);
        }
    }

    public class AllFieldsContractResolver : DefaultContractResolver
    {
        protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var props = type
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(p => base.CreateProperty(p, memberSerialization))
                .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(f => base.CreateProperty(f, memberSerialization)))
                .Where(p => !p.PropertyName.Contains("k__BackingField"))
                .ToList();
            props.ForEach(p => { p.Writable = true; p.Readable = true; });
            return props;
        }
    }

}
