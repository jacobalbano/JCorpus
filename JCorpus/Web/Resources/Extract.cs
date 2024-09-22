using Common;
using Common.Addins.Extract;
using Common.Addins.Search;
using Common.Configuration;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;
using JCorpus.DI;
using JCorpus.Job;
using JCorpus.Jobs;
using JCorpus.Utility;
using JCorpus.Web.Transit;
using JCorpus.Web.Transit.Schema;
using NodaTime;
using System.Collections.Concurrent;

namespace JCorpus.Web.Resources;

internal class Extract : IWebResource
{
    [ResourceMethod(RequestMethod.POST, "Enumerate")]
    public Result<TransitJob> DoSearch(SourceEnumerateJobParams searchParams)
        => new Result<TransitJob>(jobs.RunWrapped("SourceEnumerate", job => DoSearch(searchParams, job)).ToTransit())
        .Status(ResponseStatus.Accepted);

    [ResourceMethod(RequestMethod.GET, "Enumerate/:jobId")]
    public Result<TransitJobResult<TransitPage<CorpusWorkResource>>> GetResources(Guid jobId, int page = 1)
        => jobs.GetPendingResult(jobId, () =>
        {
            TransitPage<CorpusWorkResource> results = default;
            bool found = resourceResultCache.TryGetValue(jobId, out var hits);
            if (found) results = TransitPage<CorpusWorkResource>.OfSize(hits, 20, page);
            return (found, results);
        });

    [ResourceMethod]
    public IEnumerable<TransitJob> Get() => jobs.Select(x => x.ToTransit());

    [ResourceMethod(RequestMethod.POST)]
    public TransitJob Job(ExtractJobParams jobParams)
        => jobs.RunWrapped("Extract", job => RunExtract(jobParams, job)).ToTransit();

    [ResourceMethod(RequestMethod.GET, ":jobId")]
    public Result<TransitJobResult<ExtractReport>> GetReport(Guid jobId)
        => jobs.GetPendingResult(jobId, () => (reportCache.TryGetValue(jobId, out var result), result));

    [ResourceMethod(RequestMethod.DELETE, "Enumerate/:jobId")]
    public void Cancel(Guid jobId) => jobs.CancelJob(jobId);

    private async Task RunExtract(ExtractJobParams jobParams, ILongRunningJob job)
    {
        reportCache.TryAdd(job.Id, await pipelineFactory.Create()
            .Run(jobParams, job.Token));
    }

    public Extract(LongRunningJobRegistry jobs, IFactory<ExtractJob> pipelineFactory, IFactory<SourceEnumerateJob> enumeratorFactory)
    {
        this.jobs = jobs;
        this.pipelineFactory = pipelineFactory;
        this.enumeratorFactory = enumeratorFactory;
        PeriodicJob.Run(pruneThreshold, Cleanup);
    }

    private Task Cleanup()
    {
        foreach (var id in jobs.Prune(olderThan: pruneThreshold))
            _ = resourceResultCache.TryRemove(id, out _) || reportCache.TryRemove(id, out _);

        return Task.CompletedTask;
    }

    private async Task DoSearch(SourceEnumerateJobParams searchParams, ILongRunningJob job)
    {
        resourceResultCache.TryAdd(job.Id, await enumeratorFactory.Create()
            .Run(searchParams, job.Token));
    }

    private readonly LongRunningJobRegistry jobs;
    private readonly IFactory<SourceEnumerateJob> enumeratorFactory;
    private readonly IFactory<ExtractJob> pipelineFactory;
    private readonly ConcurrentDictionary<Guid, IReadOnlyList<CorpusWorkResource>> resourceResultCache = new();
    private readonly ConcurrentDictionary<Guid, ExtractReport> reportCache = new();
    private static readonly Duration pruneThreshold = Duration.FromMinutes(15);
}
