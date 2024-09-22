using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Content;

namespace Common.Addins.Extract;

public interface IPipelineStage : IAddin
{
    /// <summary>
    /// Describes the kinds of data that this stage can accept.<br />
    /// Should be constant for all instances of a single implementation.
    /// </summary>
    public IReadOnlyList<string> InTags { get; }

    /// <summary>
    /// Describes the kinds of data that this stage will produce.<br />
    /// Should be constant for all instances of a single implementation.
    /// </summary>
    public IReadOnlyList<string> OutTags { get; }

    /// <summary>
    /// Optional tags that will be included in the <see cref="CorpusWorkMetadata"/> for the work produced by this pipeline.<br />
    /// See <see cref="KnownTags"/> for pre-defined tags that may also be used.<br />
    /// Should be constant for all instances of a single implementation.
    /// </summary>
    public IReadOnlyList<string> MetadataTags => Array.Empty<string>();
}
