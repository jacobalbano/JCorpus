using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Content;
using Common.Content.Collections;
using Common.IO;

namespace Common;

/// <summary>
/// Enumerate and examine <see cref="CorpusWork"/>s that are stored in the system.<br/>
/// Implemented by the host application as a singleton object.
/// </summary>
public interface ICorpus
{
    /// <summary>
    /// Get a list of all <see cref="CorpusWork"/>s which are currently stored in the system.<br/>
    /// Does not include works which are in the process of being extracted.
    /// </summary>
    IEnumerable<CorpusWork> GetAvailableWorks();

    /// <summary>
    /// Get the content (as a list of <see cref="CorpusEntry"/>) of a given work.
    /// </summary>
    /// <param name="corpusWorkId">The work to return contents of.</param>
    IEnumerable<CorpusEntry> GetWorkContent(CorpusWorkId corpusWorkId);

    /// <summary>
    /// Attempts to get some supplemental content associated with the given <see cref="CorpusWork"/>.<br />
    /// <example>
    /// For example, to attempt to retrieve a <see cref="FuriganaCollection"/>:
    /// <code>
    /// if (corpus.TryGetValue(workId, out FuriganaCollection furigana))
    ///     ; // this work includes furigana
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T">The type of corpus content to attempt to find.</typeparam>
    /// <param name="corpusWorkId">The ID of the work that may contain extra content.</param>
    /// <param name="extraContent">A new instance of <typeparamref name="T"/>. If this method returned true, it will be populated from the storage directory.</param>
    /// <returns>Whether the content was successfully found.</returns>
    bool TryGetWorkExtraContent<T>(CorpusWorkId corpusWorkId, out T extraContent)
        where T : ICorpusContent, new();

    /// <summary>
    /// Return an <see cref="IVirtualFs"/> representing the storage directory for this work.<br />
    /// The directory will be read-only.
    /// </summary>
    /// <param name="corpusWorkId">The work ID to return the storage directory for.</param>
    IVirtualFs GetWorkStorageDirectory(CorpusWorkId corpusWorkId);
}