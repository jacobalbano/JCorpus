using Common.Addins.Extract;
using Common.Addins.Search;
using Common.Configuration;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;
using JCorpus.DI;
using JCorpus.Jobs;
using JCorpus.Utility;
using JCorpus.Web.Transit;
using JCorpus.Web.Transit.Schema;
using NodaTime;
using System.Collections.Concurrent;

namespace JCorpus.Web.Resources;

internal class Search : IWebResource
{
    [ResourceMethod]
    public IEnumerable<TransitJob> Get() => jobs.Select(x => x.ToTransit());

    [ResourceMethod(RequestMethod.POST)]
    public Result<TransitJob> DoSearch(SearchJobParams searchParams)
        => new Result<TransitJob>(jobs.RunWrapped("Search", job => DoSearch(searchParams, job)).ToTransit())
        .Status(ResponseStatus.Accepted);

    [ResourceMethod(RequestMethod.GET, ":jobId")]
    public Result<TransitJobResult<TransitPage<SearchHit>>> Get(Guid jobId, int page = 1)
        => jobs.GetPendingResult(jobId, () =>
        {
            TransitPage<SearchHit> results = default;
            bool found = resultCache.TryGetValue(jobId, out var hits);
            if (found) results = TransitPage<SearchHit>.OfSize(hits, 20, page);
            return (found, results);
        });

    [ResourceMethod(RequestMethod.DELETE, ":jobId")]
    public void Cancel(Guid jobId) => ResourceUtility.CancelJob(jobs, jobId);

    public Search(LongRunningJobRegistry jobs, IFactory<SearchJob> searchFactory)
    {
        this.jobs = jobs;
        this.searchFactory = searchFactory;
        PeriodicJob.Run(pruneThreshold, Cleanup);
    }

    private Task Cleanup()
    {
        foreach (var id in jobs.Prune(olderThan: pruneThreshold))
            resultCache.TryRemove(id, out _);

        return Task.CompletedTask;
    }

    private async Task DoSearch(SearchJobParams searchParams, ILongRunningJob job)
    {
        resultCache.TryAdd(job.Id, await searchFactory.Create()
            .Run(searchParams, job.Token));
    }

    private readonly LongRunningJobRegistry jobs;
    private readonly IFactory<SearchJob> searchFactory;
    private readonly ConcurrentDictionary<Guid, IReadOnlyList<SearchHit>> resultCache = new();
    private static readonly Duration pruneThreshold = Duration.FromMinutes(15);
}
