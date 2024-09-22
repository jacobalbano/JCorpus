using Common;
using Common.Configuration;
using Common.Content;
using Common.DI;
using Common.Addins.Accelerate;
using Common.Addins.Analyze;
using JCorpus.Implementation;
using JCorpus.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Jobs;

/// <summary>
/// Parameters for the Accelerate job.
/// </summary>
/// <param name="Accelerator">Configuration specifying the <see cref="IAnalysis"/> implementation to use.</param>
/// <param name="WorkIds">An optional list of IDs to run this analysis over. If not supplied, all eligible <see cref="CorpusWork"/>s will be included.</param>
[SchemaDescribe]
record class AccelerateJobParams(
    ObjectConfiguration<IAccelerator> Accelerator,
    CorpusWorkId[] WorkIds = null
);

[AutoDiscover(AutoDiscoverOptions.Transient)]
internal class AccelerateJob
{
    public async Task Run(AccelerateJobParams jobParams, CancellationToken ct)
    {
        var works = corpus.GetAvailableWorks();
        if (jobParams.WorkIds != null)
            works = works.Where(x => jobParams.WorkIds.Contains(x.UniqueId));

        foreach (var work in works.TakeWhile(_ => !ct.IsCancellationRequested))
        {
            var dir = corpus.GetWritableDirForWork(work);
            var accelerator = jobParams.Accelerator.Create(services);
            var acceleratorFile = dir.File(accelerator.ContentObject.ContentFileName);
            if (acceleratorFile.Exists) continue;

            logger.LogInformation("Accelerating {workId}", work.UniqueId);
            accelerator.Accelerate(work, corpus.GetWorkContent(work.UniqueId));
            accelerator.ContentObject.Write(acceleratorFile);
            logger.LogInformation("Done");
        }
    }

    public AccelerateJob(IServiceProvider services, ILogger<AccelerateJob> logger, ICorpus corpus)
    {
        // TODO: improve DI to allow for multiple keys to the same singleton
        this.corpus = (Corpus)corpus;
        this.services = services;
        this.logger = logger;
    }

    private readonly Corpus corpus;
    private readonly IServiceProvider services;
    private readonly ILogger logger;
}
