using Common.Configuration.Schema;

namespace JCorpus.Web.Schema;

record class ConfigPropertyDescriptor(
    string Name,
    SchemaTypeDefinition Type
);
