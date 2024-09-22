using Common.Addins.Accelerate;
using Common.Configuration;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;
using JCorpus.DI;
using JCorpus.Jobs;
using JCorpus.Web.Transit;
using JCorpus.Web.Transit.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Web.Resources;

internal class Accelerate : IWebResource
{
    [ResourceMethod]
    public IEnumerable<TransitJob> Get() => jobs.Select(x => x.ToTransit());

    [ResourceMethod(RequestMethod.POST)]
    public Result<TransitJob> Run(AccelerateJobParams jobParams)
        => new Result<TransitJob>(jobs.RunWrapped(nameof(Accelerate), job => DoAccelerate(jobParams, job)).ToTransit())
        .Status(ResponseStatus.Accepted);

    [ResourceMethod(RequestMethod.GET, ":jobId")]
    public Result<TransitJob> Get(Guid jobId) => jobs.GetPendingResult(jobId);

    [ResourceMethod(RequestMethod.DELETE, ":jobId")]
    public void Cancel(Guid jobId) => jobs.CancelJob(jobId);

    private Task DoAccelerate(AccelerateJobParams jobParams, ILongRunningJob job)
    {
        return factory.Create().Run(jobParams, job.Token);
    }

    public Accelerate(LongRunningJobRegistry jobs, IFactory<AccelerateJob> factory)
    {
        this.jobs = jobs;
        this.factory = factory;
    }

    private readonly LongRunningJobRegistry jobs;
    private readonly IFactory<AccelerateJob> factory;
}
