using Common.Configuration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JCorpus.Web.Schema;

record class EnumTypeDefinition(
    [property: JsonPropertyName("$enum")]
    string[] Options
) : SchemaTypeDefinition("string")
{
    public class Handler : IPropertyTypeHandler
    {
        bool IPropertyTypeHandler.TryCreateTypeDefinition(Type type, out SchemaTypeDefinition def)
        {
            def = default;
            if (!type.IsEnum) return false;

            def = new EnumTypeDefinition(Enum.GetNames(type));
            return true;
        }
    }
};
