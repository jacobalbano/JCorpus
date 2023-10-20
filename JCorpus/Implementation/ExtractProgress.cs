using System.Text.Json;
using Common.Content;
using Common.DI;
using Common.Utility;
using Danbo.Utility;
using JCorpus.Persistence;
using JCorpus.Persistence.Models;
using Utility;

namespace JCorpus.Implementation;

[AutoDiscover(AutoDiscoverOptions.Singleton, ImplementationFor = typeof(IExtractProgress))]
internal class ExtractProgress : IExtractProgress, IDisposable
{
    public ExtractProgress(Database.ISession session, UniqueId corpusWorkId)
    {
        this.session = session;
        var dbProgress = session.Select<DbExtractProgress>()
            .Where(x => x.CorpusWorkId == corpusWorkId)
            .FirstOrDefault() ?? new() { CorpusWorkId = corpusWorkId, Ids = ids };

        progress = dbProgress with { Ids = ids = dbProgress.Ids.ToList() };
    }

    public void Reset()
    {
        ids.Clear();
    }

    public void Dispose()
    {
        session.InsertOrUpdate(progress);
    }

    public IEnumerable<string> GetIds() => ids;

    public void ReportProgress(string identifier)
    {
        if (!ids.Contains(identifier))
            ids.Add(identifier);
    }

    private readonly List<string> ids = new();
    private readonly DbExtractProgress progress;
    private readonly Database.ISession session;
}
