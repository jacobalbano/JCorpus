using Common.DI;
using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Content;

namespace Common;

/// <summary>
/// Represents some supplemental content that should be stored alongside of an extracted <see cref="CorpusWork"/>.
/// </summary>
[AutoDiscover(AutoDiscoverOptions.Implementations | AutoDiscoverOptions.Scoped)]
public interface ICorpusContent : IFreezable
{
    /// <summary>
    /// The name of the content file as it will exist in the archive.
    /// Should return a constant result for all instances of a given implementation.
    /// </summary>
    string ContentFileName { get; }

    /// <summary>
    /// Read the contents of the file from disk. Should be explicitly implemented to reduce surface area.
    /// </summary>
    /// <param name="inFile"></param>
    void Read(IVirtualFile inFile);

    /// <summary>
    /// Write the contents of the file from disk. Should be explicitly implemented to reduce surface area.
    /// </summary>
    void Write(IVirtualFile outFile);
}