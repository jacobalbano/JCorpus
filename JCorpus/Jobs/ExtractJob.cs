using Common;
using Common.IO;
using JCorpus.Implementation.IO.Filesystem;
using JCorpus.Implementation;
using JCorpus.Persistence.Models;
using JCorpus.Utility;
using System.Text.Json;
using JCorpus.Persistence;
using JCorpus.Web.Resources;
using JCorpus.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;
using Common.IO.Zip;
using Common.Configuration;
using Common.Content;
using Common.Addins.Extract;
using Common.DI;
using Utility.IO;
using System.Xml;
using System.Text.Json.Serialization;
using System.Net.WebSockets;

namespace JCorpus.Jobs;

[JsonConverter(typeof(JsonStringEnumConverter))]
enum FilterIdMode
{
    Include,
    Exclude
}

/// <summary>
/// Parameters for the Extract job.
/// </summary>
/// <param name="Source">
/// Configuration specifying the <see cref="ICorpusWorkSource"/> implementation to use.<br />
/// Filtering should be configured on this object.
/// </param>
/// <param name="Extractor">Configuration specifying the <see cref="ICorpusWorkExtractor"/> implementation to use.</param>
[SchemaDescribe]
record class ExtractJobParams(
    ObjectConfiguration<ICorpusWorkSource> Source,
    ObjectConfiguration<ICorpusWorkExtractor> Extractor,
    IReadOnlyList<CorpusWorkId> FilterIds,
    FilterIdMode FilterMode
);

record class ExtractReport(
    IReadOnlyList<CorpusWorkId> Completed,
    IReadOnlyList<CorpusWorkId> Failed
);

[AutoDiscover(AutoDiscoverOptions.Transient)]
internal class ExtractJob
{
    public async Task<ExtractReport> Run(ExtractJobParams jobParams, CancellationToken ct)
    {
        List<CorpusWorkId> completed = new(), failed = new();
        using var jobScope = services.CreateScope();
        var source = jobParams.Source.Create(jobScope.ServiceProvider);
        var availableWorks = source.EnumerateAvailableWorks(ct)
            .Where(x => (jobParams.FilterMode == FilterIdMode.Include) == jobParams.FilterIds.Contains(x.UniqueId) )
            .TakeWhile(_ => !ct.IsCancellationRequested);

        await foreach (var book in availableWorks)
        {
            using var bookScope = services.CreateScope();
            using var dbSession = db.BeginSession();
            using var progress = new ExtractProgress(dbSession, book.UniqueId);
            var dbWork = dbSession.Select<DbCorpusWork>()
                .Where(x => x.CorpusWorkId == book.UniqueId)
                .FirstOrDefault() ?? new() { Status = Status.New, CorpusWorkId = book.UniqueId };

            try
            {
                switch (dbWork.Status)
                {
                    case Status.New:
                        dbSession.InsertOrUpdate(dbWork with { Status = Status.ExtractStarted });
                        break;
                    case Status.ExtractStarted:
                        break;
                    case Status.ExtractCompleted:
                        continue;
                    case Status.ExtractFailed:
                        logger.LogInformation("Extract previously failed; retrying");
                        progress.Reset();
                        dbWork = dbWork with { Status = Status.ExtractStarted };
                        break;
                    default:
                        throw new NotImplementedException($"Switch branch {dbWork.Status} not implemented");
                }

                logger.LogInformation("Processing book {uri}", book.Uri);

                var extractor = jobParams.Extractor.Create(bookScope.ServiceProvider);
                var work = new CorpusWork(book.UniqueId, new(
                    new(jobParams.Source.PluginName, jobParams.Source.AddinName),
                    new (jobParams.Extractor.PluginName, jobParams.Extractor.AddinName),
                    source.MetadataTags.Concat(extractor.MetadataTags).ToList()
                ));

                var workOutput = extractDir.Directory((string)work.UniqueId);
                workOutput.Create();
                using var content = source.DownloadWork(book.Uri);
                using var context = extractor.Extract(content, progress);

                foreach (var extra in context.GetExtraContent())
                {
                    var file = workOutput.File(extra.ContentFileName);
                    if (file.Exists) extra.Read(file);
                }

                var entries = new CorpusEntryCollection();
                var entriesFile = workOutput.File(entries.ContentFileName);
                if (entriesFile.Exists) (entries as ICorpusContent).Read(entriesFile);

                await foreach (var entry in context.EnumerateEntries(ct))
                    entries.Add(entry);

                foreach (var extra in context.GetExtraContent())
                    extra.Write(workOutput.File(extra.ContentFileName));
                (entries as ICorpusContent).Write(entriesFile);
                CorpusWorkMetadata.Write(
                    work.Metadata,
                    workOutput.File(CorpusWorkMetadata.ContentFileName)
                );

                if (ct.IsCancellationRequested)
                    break;

                var finalOutput = corpus.GetWritableDirForWork(work);
                foreach (var path in workOutput.EnumerateFiles(searchOption: SearchOption.AllDirectories))
                    workOutput.File(path).CopyTo(finalOutput, overwrite: true);
                workOutput.Delete();

                dbWork = dbWork with { Status = Status.ExtractCompleted };
                completed.Add(book.UniqueId);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while extracting");
                dbWork = dbWork with { Status = Status.ExtractFailed };
                failed.Add(book.UniqueId);
            }
            finally
            {
                dbSession.Update(dbWork);
            }
        }

        logger.LogInformation("Done");
        return new(completed, failed);
    }

    public ExtractJob(IServiceProvider services, ICorpus corpus, IPersistentDirectory<ExtractJob> dir, ILogger<Extract> logger, Database db)
    {
        // TODO: improve DI to allow for multiple keys to the same singleton
        this.corpus = (Corpus) corpus;
        this.services = services;
        this.extractDir = dir;
        this.logger = logger;
        this.db = db;
    }

    private readonly IServiceProvider services;
    private readonly IPersistentDirectory<ExtractJob> extractDir;
    private readonly Corpus corpus;
    private readonly ILogger logger;
    private readonly Database db;
}
