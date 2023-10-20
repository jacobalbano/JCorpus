using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.IO;

/// <summary>
/// Represents a virtual directory. May itself be contained by another <see cref="IVirtualFs"/>
/// </summary>
public interface IVirtualFs : IVirtualFsObject
{
    /// <summary>
    /// This object's name.
    /// If this object represents a root filesystem, identifies the filesystem itself.
    /// If it represents a subdirectory, this name will be <seealso cref="PathUtility.IsQualified(DirectoryPath)">unqualified</seealso>.
    /// </summary>
    DirectoryPath DirectoryName { get; }

    /// <summary>
    /// Seek a file within this directory.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>A <see cref="IVirtualFile"/> representing the requested path. Will not return null, but the file may not <seealso cref="IVirtualFsObject.Exists">exist</seealso>.</returns>
    IVirtualFile File(FilePath path);

    /// <summary>
    /// Seek a subdirectory.
    /// </summary>
    /// <param name="path">The path to the directory.</param>
    /// <returns>A <see cref="IVirtualFs"/> representing the requested path. Will not return null, but the directory may not <seealso cref="IVirtualFsObject.Exists">exist</seealso>.</returns>
    IVirtualFs Directory(DirectoryPath path);

    /// <summary>Create the directory that this object represents, if it doesn't exist already.</summary>
    void Create();

    /// <summary>
    /// Enumerate files in this directory (and optionally its descendents).
    /// </summary>
    /// <param name="pattern">A search pattern to restrict filenames. Match any number of characters with *, and any single character with ?.</param>
    /// <param name="searchOption">Whether to search immediate files only or to recurse into subdirectories.</param>
    /// <returns>Paths to files which match the criteria.</returns>
    IEnumerable<FilePath> EnumerateFiles(string pattern = MatchAllFilesPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly);

    /// <summary>
    /// Enumerate subdirectories in this directory (and optionally its descendents).
    /// </summary>
    /// <param name="searchOption">Whether to search immediate files only or to recurse into subdirectories.</param>
    /// <returns>Paths to files which match the criteria.</returns>
    IEnumerable<DirectoryPath> EnumerateDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly);

    /// <summary>The standard path separator. Forward slash on all platforms.</summary>
    public const char PathSeparator = '/';

    /// <summary>The default pattern to match all files when enumerating.</summary>
    public const string MatchAllFilesPattern = "*";
}