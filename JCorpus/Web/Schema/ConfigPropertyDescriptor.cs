namespace Common.Configuration.Schema;

record class ConfigPropertyDescriptor(
    string Name,
    SchemaTypeDefinition Type
);
