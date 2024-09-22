using Common.Configuration.Schema;

namespace JCorpus.Web.Schema;

internal interface IPropertyTypeHandler
{
    public bool TryCreateTypeDefinition(Type type, out SchemaTypeDefinition def);
}