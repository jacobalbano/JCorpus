using Common.Addins.Analyze;
using Common.Configuration;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;
using JCorpus.DI;
using JCorpus.Jobs;
using JCorpus.Web.Transit;
using System.Collections.Concurrent;

namespace JCorpus.Web.Resources;

internal class Analyze : IWebResource
{
    [ResourceMethod]
    public IEnumerable<TransitJob> Get() => jobs.Select(x => x.ToTransit());

    [ResourceMethod(RequestMethod.POST)]
    public Result<TransitJob> Run(AnalyzeJobParams analyzeParams)
        => new Result<TransitJob>(jobs.RunWrapped(nameof(Analyze), job => DoAnalyze(analyzeParams, job)).ToTransit())
        .Status(ResponseStatus.Accepted);

    [ResourceMethod(RequestMethod.GET, ":jobId")]
    public Result<TransitJob> Get(Guid jobId) => jobs.GetPendingResult(jobId);

    [ResourceMethod(RequestMethod.GET, ":jobId/Download")]
    public IResponse Result(Guid jobId, IRequest request)
    {
        if (!jobs.TryGet(jobId, out var job))
            throw new ProviderException(ResponseStatus.NotFound, "Not found");

        if (!resultCache.TryGetValue(jobId, out var result))
            throw new ProviderException(ResponseStatus.Gone, "Gone");

        var ms = new MemoryStream(result.ResultData);
        return request.Respond()
            .Content(ms, () => ms.CalculateChecksumAsync())
            .Type(ContentType.ApplicationForceDownload)
            .Header("content-disposition", $"attachment; filename=\"{result.Filename}\"")
            .Build();
    }

    [ResourceMethod(RequestMethod.DELETE, ":jobId")]
    public void Cancel(Guid jobId) => jobs.CancelJob(jobId);

    private async Task DoAnalyze(AnalyzeJobParams jobParams, ILongRunningJob job)
    {
        resultCache.TryAdd(job.Id, await factory.Create()
            .Run(jobParams, job.Token));
    }

    public Analyze(LongRunningJobRegistry jobs, IFactory<AnalyzeJob> factory)
    {
        this.jobs = jobs;
        this.factory = factory;
    }

    private readonly ConcurrentDictionary<Guid, AnalysisResult> resultCache = new();
    private readonly LongRunningJobRegistry jobs;
    private readonly IFactory<AnalyzeJob> factory;
}
