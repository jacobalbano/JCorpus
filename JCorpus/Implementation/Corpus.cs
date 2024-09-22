using Common;
using Common.Content;
using Common.DI;
using Common.IO;
using JCorpus.Implementation.IO.Filesystem;
using JCorpus.Persistence;
using JCorpus.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utility.IO;

namespace JCorpus.Implementation;

[AutoDiscover(AutoDiscoverOptions.Singleton, ImplementationFor = typeof(ICorpus))]
internal class Corpus : ICorpus
{
    public Corpus(Database database, IPersistentDirectory<Corpus> workingDir)
    {
        this.database = database;
        this.workingDir = workingDir;
    }

    public IEnumerable<CorpusWork> GetAvailableWorks()
    {
        foreach (var path in workingDir.EnumerateDirectories(SearchOption.TopDirectoryOnly))
            yield return new CorpusWork(
                (string) path,
                null/*CorpusWorkMetadata.Read(workingDir
                    .Directory(path)
                    .File(CorpusWorkMetadata.ContentFileName)
                )*/
            );
    }

    public IEnumerable<CorpusEntry> GetWorkContent(CorpusWorkId corpusWorkId)
    {
        TryGetWorkExtraContent(corpusWorkId, out CorpusEntryCollection result);
        return result;
    }

    public bool TryGetWorkExtraContent<T>(CorpusWorkId corpusWorkId, out T extraContent) where T : ICorpusContent, new()
    {
        extraContent = new T();
        var file = workingDir.Directory((string)corpusWorkId)
            .File(extraContent.ContentFileName);
        if (!file.Exists) return false;

        try { extraContent.Read(file); }
        catch (Exception) { return false; }
        extraContent.Freeze();
        return true;
    }

    public IVirtualFs GetWorkStorageDirectory(CorpusWorkId corpusWorkId)
    {
        return workingDir.Directory((string)corpusWorkId)
            .AsReadOnly();
    }

    public IVirtualFs GetWritableDirForWork(CorpusWork work)
    {
        return new VirtualFs(workingDir.Directory((string)work.UniqueId)
            .GetFullyQualifiedPath());
    }

    private readonly Database database;
    private readonly IPersistentDirectory<Corpus> workingDir;
}
