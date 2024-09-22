using Common.Configuration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.Configuration;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class SchemaDescribeAttribute : Attribute
{
    public Type ConverterType { get; init; }
    public JsonTypename Typename { get; init; } = JsonTypename.None;
}

public interface ISchemaDescriptor
{
    SchemaTypeDefinition GetDefinition();
}

public enum JsonTypename
{
    None,
    String,
    Array,
    Boolean,
    Number,
    Object
}