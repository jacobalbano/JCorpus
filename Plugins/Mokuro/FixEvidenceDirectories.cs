using Common;
using Common.Configuration;
using Common.Content;
using Common.DI;
using Common.IO;
using Common.Addins.Accelerate;
using Microsoft.Extensions.Logging;
using MokuroWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mokuro;

/// <summary>
/// Convert a folder of images and json files (the original format for a mokuro extract) into a <see cref="EvidenceCollection"/>
/// </summary>
[AutoDiscover(AutoDiscoverOptions.Transient)]
public class FixEvidenceDirectories : IAccelerator
{
    public ICorpusContent ContentObject => evidence;

    public FixEvidenceDirectories(ICorpus corpus, ILogger<FixEvidenceDirectories> logger)
    {
        this.corpus = corpus;
        this.logger = logger;
    }

    public void Accelerate(CorpusWork work, IEnumerable<CorpusEntry> content)
    {
        var dir = corpus.GetWorkStorageDirectory(work.UniqueId);
        corpus.TryGetWorkExtraContent(work.UniqueId, out EvidenceCollection archive);
        logger.LogInformation("Processing {work}", work.UniqueId);

        archive.ReadFromDirectory(dir);
        var output = dir.File(archive.ContentFileName);
        (archive as ICorpusContent).Write(output);

        foreach (var (filename, _) in archive.Images)
            dir.File($"{filename}.jpg").Delete();

        foreach (var (filename, _) in archive.PageData)
            dir.File(filename).Delete();

        archive.Dispose();
        logger.LogInformation("Done");
    }

    private readonly EvidenceCollection evidence = new();
    private readonly ICorpus corpus;
    readonly ILogger logger;
}
