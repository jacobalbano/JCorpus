using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Content;
using Common.DI;
using Common.Utility;

namespace Common.Addins.Extract;

/// <summary>
/// Provides a context for <see cref="CorpusEntry">CorpusEntries</see> to be extracted from a <see cref="CorpusWorkResource" />.<br />
/// See <see cref="ICorpusWorkExtractorContext"/> for more details on how to implement this interface.
/// </summary>
[AutoDiscover(AutoDiscoverOptions.Implementations | AutoDiscoverOptions.Transient)]
public interface ICorpusWorkExtractor : IPipelineStage
{
    /// <summary>
    /// Describes a context in which <see cref="CorpusEntry"/> extraction will be performed.
    /// The context will be disposed upon completion, at which point intermediary assets should be cleaned up.
    /// </summary>
    public interface IContext : IDisposable
    {
        /// <summary>
        /// Extract and return a list of <see cref="CorpusEntry"/>.
        /// </summary>
        public IAsyncEnumerable<CorpusEntry> EnumerateEntries(CancellationToken ct);

        /// <summary>
        /// Return any <see cref="ICorpusContent"/> objects being used by this extraction context.<br />
        /// This method will be called at the beginning of the extraction process (so content can be loaded if it exists) and at the end (to save results).
        /// </summary>
        public IEnumerable<ICorpusContent> GetExtraContent();
    }

    /// <summary>
    /// Returns a context in which the extraction work will be done.
    /// </summary>
    ///<param name="stream">The stream which the work will be read from.</param>
    ///<param name="progress">An object which can be used to report and query progress.</param>
    /// <returns>The work context</returns>
    IContext Extract(Stream stream, IExtractProgress progress);

    /// <summary>This interface represents the end of the pipeline.</summary>
    IReadOnlyList<string> IPipelineStage.OutTags => Array.Empty<string>();
}
