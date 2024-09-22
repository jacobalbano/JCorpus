using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Common.Content;
using Common.DI;
using Common.Addins.Extract;

namespace Common.Addins.Extract;

/// <summary>
/// Represents a source that works may be acquired from.
/// May represent a website (like aozorabunko), a folder scheme (like Calibre's library structure), etc.
/// </summary>
[AutoDiscover(AutoDiscoverOptions.Implementations | AutoDiscoverOptions.Scoped)]
public interface ICorpusWorkSource : IPipelineStage
{
    /// <summary>
    /// Enumerates the <see cref="CorpusWork"/>s that may be acquired from this source.
    /// Filtering, searching etc must be specified on the implementor.
    /// </summary>
    IAsyncEnumerable<CorpusWorkResource> EnumerateAvailableWorks(CancellationToken ct);

    /// <summary>
    /// Downloads a stream containing the work's data.
    /// </summary>
    /// <param name="uri">The URI of the work.</param>
    /// <returns>The data stream.</returns>
    Stream DownloadWork(string uri);

    /// <summary>This interface represents the beginning of the pipeline.</summary>
    IReadOnlyList<string> IPipelineStage.InTags => Array.Empty<string>();
}
