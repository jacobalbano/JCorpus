using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.IO;

/// <summary>
/// Provides access to file operations inside a <see cref="IVirtualFs"/>
/// </summary>
public interface IVirtualFile : IVirtualFsObject
{
    /// <summary>
    /// This object's name.
    /// Guaranteed to be <seealso cref="PathUtility.IsQualified(DirectoryPath)">unqualified</seealso>.
    /// </summary>
    FilePath Filename { get; }

    /// <summary>
    /// Open the file as a stream.
    /// Depending on the implementation, not all <see cref="FileMode"/>s will be supported.
    /// </summary>
    /// <param name="mode">How to open the file stream.</param>
    /// <returns>The opened stream. Disposal is the responsibility of the caller.</returns>
    Stream Open(FileMode mode);
}
