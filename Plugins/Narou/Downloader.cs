using Common;
using Common.Configuration;
using Common.Content;
using Common.Addins.Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Addins;
using Common.IO;
using Utility.IO;

namespace Narou;

public class Downloader : ICorpusWorkSource
{
    public record class Config();

    public IReadOnlyList<string> OutTags => Constants.Tags;

    public Stream DownloadWork(string uri)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<CorpusWorkResource> EnumerateAvailableWorks(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
