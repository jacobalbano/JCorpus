using Common.Content;
using JCorpus.Persistence.TypeConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Persistence.Models;

internal record class DbExtractProgress : ModelBase
{
    [Indexed, BsonConverter(typeof(UniqueIdConverter))]
    public UniqueId CorpusWorkId { get; init; }

    public IReadOnlyList<string> Ids { get; init; }
}