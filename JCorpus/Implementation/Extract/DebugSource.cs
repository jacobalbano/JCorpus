using Common.Addins.Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Implementation.Extract;

#if DEBUG
internal class DebugSource : ICorpusWorkSource
{
    public IReadOnlyList<string> OutTags => throw new NotImplementedException();

    public Stream DownloadWork(string uri) => throw new NotImplementedException("Debug implementation cannot download works");

    public async IAsyncEnumerable<CorpusWorkResource> EnumerateAvailableWorks([EnumeratorCancellation] CancellationToken ct)
    {
        for (int i = 1; i < 25; i++)
        {
            yield return new CorpusWorkResource("TEST-" + i, "", new ResourceField[]
            {
                new(KnownFields.Author, "Test, Author"),
                new(KnownFields.Title, "Test vol. " + i),
            }, new[] { "Manga" });
        }
    }
}
#endif