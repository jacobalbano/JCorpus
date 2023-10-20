using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Common.Content;
using Common.DI;

namespace Common;

/// <summary>
/// Enumerates the works available from a given source.
/// May represent a website (like aozorabunko), a folder scheme (like Calibre's library structure), etc.
/// </summary>
[AutoDiscover(AutoDiscoverOptions.Implementations | AutoDiscoverOptions.Scoped)]
public interface ICorpusWorkEnumerator
{
    /// <summary>
    /// Enumerates the <see cref="CorpusWork"/>s that may be acquired from this source.
    /// Filtering, searching etc must be specified on the implementor.
    /// </summary>
    IAsyncEnumerable<CorpusWork> GetAvailableWorks(CancellationToken ct);
}
