using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Content;

namespace Common;

/// <summary>
/// Describes a context in which <see cref="CorpusEntry"/> extraction will be performed.
/// The context will be disposed upon completion, at which point intermediary assets should be cleaned up.
/// </summary>
public interface ICorpusWorkExtractorContext : IDisposable
{
    /// <summary>
    /// Extract and return a list of <see cref="CorpusEntry"/>
    /// </summary>
    public IAsyncEnumerable<CorpusEntry> EnumerateEntries(CancellationToken ct);
}
