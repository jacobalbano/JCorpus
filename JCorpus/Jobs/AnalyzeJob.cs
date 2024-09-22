using Common;
using Common.Configuration;
using Common.Content;
using Common.DI;
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
/// Parameters for the Analysis job.
/// </summary>
/// <param name="Analysis">Configuration specifying the <see cref="IAnalysis"/> implementation to use.</param>
/// <param name="WorkIds">An optional list of IDs to run this analysis over. If not supplied, all eligible <see cref="CorpusWork"/>s will be included.</param>
[SchemaDescribe]
public record class AnalyzeJobParams(
    ObjectConfiguration<IAnalysis> Analysis,
    CorpusWorkId[] WorkIds = null
);

public record class AnalysisResult(
    string Filename,
    byte[] ResultData
);

[AutoDiscover(AutoDiscoverOptions.Transient)]
internal class AnalyzeJob
{
    public async Task<AnalysisResult> Run(AnalyzeJobParams jobParams, CancellationToken ct)
    {
        logger.LogInformation("Beginning analysis");
        var works = corpus.GetAvailableWorks();
        if (jobParams.WorkIds != null)
            works = works.Where(x => jobParams.WorkIds.Contains(x.UniqueId));

        var analysis = jobParams.Analysis.Create(services);
        using var data = analysis.Run(works.TakeWhile(_ => !ct.IsCancellationRequested));
        using var ms = new MemoryStream();
        data.CopyTo(ms);
        
        logger.LogInformation("Done");
        return new(analysis.Filename, ms.GetBuffer());
    }

    public AnalyzeJob(IServiceProvider services, ICorpus corpus, ILogger<AnalyzeJob> logger)
    {
        // TODO: improve DI to allow for multiple keys to the same singleton
        this.corpus = (Corpus) corpus;
        this.services = services;
        this.logger = logger;
    }

    private readonly IServiceProvider services;
    private readonly Corpus corpus;
    private readonly ILogger logger;
}
