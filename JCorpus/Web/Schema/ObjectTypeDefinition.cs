using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common.Configuration.Schema;
using JCorpus.Web.Transit.Schema;

namespace JCorpus.Web.Schema;

record class ObjectTypeDefinition(
    [property: JsonPropertyName("$schema")]
    TransitConfigSchema Schema
) : SchemaTypeDefinition("object");