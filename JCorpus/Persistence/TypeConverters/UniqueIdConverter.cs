using Common.Content;
using JCorpus.Persistence.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Persistence.TypeConverters;

internal class CorpusEntryIdConverter : IBsonConverter<CorpusEntryId>
{
    public CorpusEntryId Deserialize(BsonValue value) => value.AsString;
    public BsonValue Serialize(CorpusEntryId value) => value.Value;
}

internal class CorpusWorkIdConverter : IBsonConverter<CorpusWorkId>
{
    public CorpusWorkId Deserialize(BsonValue value) => value.AsString;
    public BsonValue Serialize(CorpusWorkId value) => value.Value;
}
