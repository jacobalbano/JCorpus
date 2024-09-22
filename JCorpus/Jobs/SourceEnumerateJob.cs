using Common;
using Common.Addins.Extract;
using Common.Addins.Search;
using Common.Configuration;
using Common.DI;
using JCorpus.Implementation;
using JCorpus.Jobs;
using JCorpus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Job;

/// <summary>
/// Parameters for the SourceEnumerate job.
/// </summary>
/// <param name="Source">
/// Configuration specifying the <see cref="ICorpusWorkSource"/> implementation to use.<br />
/// Filtering should be configured on this object.
/// </param>
[SchemaDescribe]
record class SourceEnumerateJobParams(
    ObjectConfiguration<ICorpusWorkSource> Source
);

[AutoDiscover(AutoDiscoverOptions.Transient)]
internal class SourceEnumerateJob
{
    public async Task<IReadOnlyList<CorpusWorkResource>> Run(SourceEnumerateJobParams searchParams, CancellationToken ct)
    {
        var enumerator = searchParams.Source.Create(services);
        return await enumerator.EnumerateAvailableWorks(ct)
            .TakeWhile(_ => !ct.IsCancellationRequested)
            .ToListAsync(cancellationToken: ct);
    }

    public SourceEnumerateJob(IServiceProvider services)
    {
        this.services = services;
    }

    private readonly IServiceProvider services;
}
