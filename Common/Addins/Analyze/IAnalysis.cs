using Common.Content;
using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Addins.Analyze;

/// <summary>
/// Describes an object which performs some form of analysis over the corpus.<br />
/// At the end of the process, a data stream is returned for download or saving to disk.
/// </summary>
[AutoDiscover(AutoDiscoverOptions.Implementations | AutoDiscoverOptions.Transient)]
public interface IAnalysis : IAddin
{
    /// <summary>The filename that the resulting file may be saved with.</summary>
    public string Filename { get; }

    /// <summary>
    /// Perform analysis and return a data stream containing the results.<br />
    /// Disposal of the stream is the responsibility of the caller.
    /// </summary>
    public Stream Run(IEnumerable<CorpusWork> works);
}
