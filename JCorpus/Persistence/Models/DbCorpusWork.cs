using Common.Content;
using JCorpus.Persistence.TypeConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Persistence.Models;

public enum Status
{
    New,
    ExtractStarted,
    ExtractCompleted,
    ExtractFailed
}

internal record class DbCorpusWork : ModelBase
{
    [Indexed, BsonConverter(typeof(CorpusWorkIdConverter))]
    public CorpusWorkId CorpusWorkId { get; init; }

    public Status Status { get; init; }
}
