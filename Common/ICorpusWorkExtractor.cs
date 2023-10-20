using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Content;
using Common.DI;
using Common.Utility;

namespace Common;

/// <summary>
/// Provides a context for <see cref="CorpusEntry"/>s to be extracted from a <see cref="CorpusWork
/// See <see cref="ICorpusWorkExtractorContext"/> for more details on how to implement this interface.
/// </summary>
[AutoDiscover(AutoDiscoverOptions.Implementations | AutoDiscoverOptions.Transient)]
public interface ICorpusWorkExtractor
{
    /// <summary>
    /// Returns a context in which the extraction work will be done.
    /// </summary>
    /// <param name="work">The <see cref="CorpusWork"/> to extract from</param>
    /// <returns>The work context</returns>
    public ICorpusWorkExtractorContext Extract(CorpusWork work, IExtractProgress progress);
}
