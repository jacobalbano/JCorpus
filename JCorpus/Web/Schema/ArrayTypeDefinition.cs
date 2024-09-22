using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common.Configuration.Schema;
using JCorpus.Web.Transit.Schema;

namespace JCorpus.Web.Schema;

record class ArrayTypeDefinition(
    [property: JsonPropertyName("$subtype")]
    SchemaTypeDefinition Subtype
) : SchemaTypeDefinition("array")
{
    public class Handler : IPropertyTypeHandler
    {
        bool IPropertyTypeHandler.TryCreateTypeDefinition(Type type, out SchemaTypeDefinition def)
        {
            def = default;
            var i = type.GetInterfaces()
                .Where(x => x.IsGenericType)
                .Where(x => x.GetGenericTypeDefinition() == EnumerableOpen)
                .FirstOrDefault();
            if (i == null) return false;

            var T = i.GenericTypeArguments.First();
            if (!TransitConfigSchema.TryGetTypeDefinition(T, out var subtype))
                return false;

            def = new ArrayTypeDefinition(subtype);
            return true;
        }

        private static readonly Type EnumerableOpen = typeof(IEnumerable<>);
    }
}
