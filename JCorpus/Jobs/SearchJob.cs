using Common;
using Common.Configuration;
using Common.DI;
using Common.Addins.Search;
using JCorpus.Utility;
using Utility;
using NodaTime;

namespace JCorpus.Jobs;

[SchemaDescribe]
record class SearchJobParams(
    ObjectConfiguration<ISearch> SearchBy
);

[AutoDiscover(AutoDiscoverOptions.Transient)]
internal class SearchJob
{
    public async Task<IReadOnlyList<SearchHit>> Run(SearchJobParams searchParams, CancellationToken ct)
    {
        var searchBy = searchParams.SearchBy.Create(services);
        return corpus.GetAvailableWorks()
            .SelectMany(work => searchBy.YieldHitsInWork(corpus, work))
            .TakeWhile(_ => !ct.IsCancellationRequested)
            .ToList();
    }

    public SearchJob(ICorpus corpus, IServiceProvider services)
    {
        this.corpus = corpus;
        this.services = services;
    }

    private readonly ICorpus corpus;
    private readonly IServiceProvider services;
}
