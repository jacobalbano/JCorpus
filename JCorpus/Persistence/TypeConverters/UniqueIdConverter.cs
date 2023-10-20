using Common.Content;
using JCorpus.Persistence.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Persistence.TypeConverters;

internal class UniqueIdConverter : IBsonConverter<UniqueId>
{
    public UniqueId Deserialize(BsonValue value) => value.AsString;
    public BsonValue Serialize(UniqueId value) => value.ToString();
}
