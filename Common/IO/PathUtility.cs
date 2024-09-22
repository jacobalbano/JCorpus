using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.IO;

public static class PathUtility
{
    /// <summary>
    /// Turns a qualified <see cref="DirectoryPath"/> into a collection of unqualified paths.
    /// </summary>
    /// <param name="multiPartPath">The (potentially) qualified path.</param>
    /// <returns>The resulting collection.</returns>
    public static IEnumerable<DirectoryPath> GetPathParts(this DirectoryPath multiPartPath)
    {
        return multiPartPath.ToString()
            .Split(IVirtualFs.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new DirectoryPath(x));
    }

    /// <summary>
    /// Turns a qualified <see cref="FilePath"/> into a collection of unqualified <see cref="DirectoryPath"/>s.
    /// </summary>
    /// <param name="multiPartPath">The (potentially) qualified file path.</param>
    /// <returns>The resulting collection. Does not include the terminal <see cref="FilePath"/>; use <seealso cref="GetUnqualifiedFilePath(FilePath)"/> to get it.</returns>
    public static IEnumerable<DirectoryPath> GetPathParts(this FilePath multiPartPath)
    {
        var parts = multiPartPath.ToString()
            .Split(IVirtualFs.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new DirectoryPath(x))
            .ToList();

        return parts.Take(parts.Count - 1);
    }

    /// <summary>
    /// Get the qualified path of the directory that contains this <see cref="FilePath"/>.
    /// </summary>
    /// <param name="multiPartPath"></param>
    /// <returns></returns>
    public static DirectoryPath GetDirectoryPath(this FilePath multiPartPath)
    {
        return string.Join(IVirtualFs.PathSeparator, multiPartPath.GetPathParts());
    }

    /// <summary>
    /// Get only the terminal <see cref="FilePath"/> from a qualified path.
    /// </summary>
    /// <param name="multiPartPath">The (potentially) qualified file path.</param>
    /// <returns>The resulting unqualified file path.</returns>
    public static FilePath GetUnqualifiedFilePath(this FilePath multiPartPath)
    {
        if (!multiPartPath.IsQualified())
            return multiPartPath;

        var pathStr = multiPartPath.ToString();
        var index = pathStr.LastIndexOf(IVirtualFs.PathSeparator);
        return pathStr[(index+1)..];
    }

    /// <summary>
    /// Get the name from a <see cref="FilePath"/>.
    /// This method assumes that the final dot (.) in a filename marks the beginning of its extension.
    /// This means that for a file called "archive.tar.gz", the resulting filename will be "archive.tar".
    /// </summary>
    /// <param name="path">The FilePath to get the name from.</param>
    /// <returns>The file's name, without its extension.</returns>
    public static string GetFilenameWithoutExtension(this FilePath path)
    {
        var strName = path.GetUnqualifiedFilePath().ToString();
        var index = strName.LastIndexOf('.');
        if (index < 0) return strName;

        return strName[..index];
    }

    /// <summary>
    /// Get the extension from a <see cref="FilePath"/>.
    /// This method assumes that the final dot (.) in a filename marks the beginning of its extension.
    /// This means that for a file called "archive.tar.gz", the resulting extension will be "gz".
    /// </summary>
    /// <param name="path">The FilePath to get the name from.</param>
    /// <returns>The file's name, without its extension.</returns>
    public static string GetExtension(this FilePath path)
    {
        var strName = path.GetUnqualifiedFilePath().ToString();
        var index = strName.LastIndexOf('.');
        if (index < 0 || index == strName.Length) return strName;

        return strName[(index + 1)..];
    }

    /// <summary>
    /// Traverse upwards from a <see cref="IVirtualFile"/> and produce a full path that includes the root.
    /// </summary>
    /// <param name="file">The file to produce a path for.</param>
    /// <returns>The resulting path.</returns>
    public static FilePath GetFullyQualifiedPath(this IVirtualFile file) => string.Join(IVirtualFs.PathSeparator, TraverseNamesUpwards(file));

    /// <summary>
    /// Traverse upwards from a <see cref="IVirtualFs"/> and produce a full path that includes the root.
    /// </summary>
    /// <param name="file">The directory to produce a path for.</param>
    /// <returns>The resulting path.</returns>
    public static DirectoryPath GetFullyQualifiedPath(this IVirtualFs directory) => string.Join(IVirtualFs.PathSeparator, TraverseNamesUpwards(directory));

    /// <summary>
    /// Traverse upwards from a <see cref="IVirtualFile"/> and produce a path relative to the root.
    /// </summary>
    /// <param name="file">The file to produce a path for.</param>
    /// <returns>The resulting path.</returns>
    public static FilePath GetLocallyQualifiedPath(this IVirtualFile file) => string.Join(IVirtualFs.PathSeparator, TraverseNamesUpwards(file).Skip(1));

    /// <summary>
    /// Traverse upwards from a <see cref="IVirtualFs"/> and produce a path relative to the root.
    /// </summary>
    /// <param name="file">The directory to produce a path for.</param>
    /// <returns>The resulting path.</returns>
    public static DirectoryPath GetLocallyQualifiedPath(this IVirtualFs directory) => string.Join(IVirtualFs.PathSeparator, TraverseNamesUpwards(directory).Skip(1));

    /// <summary>Whether this path represents a plain filename or a file within a subdirectory.</summary>
    public static bool IsQualified(this DirectoryPath path) => path.ToString().Contains(IVirtualFs.PathSeparator);

    /// <summary>Whether this path represents a plain filename or a file within a subdirectory.</summary>
    public static bool IsQualified(this FilePath path) => path.ToString().Contains(IVirtualFs.PathSeparator);

    /// <summary>
    /// Throw an exception if a path is empty, whitespace-only, contains relative path segments, or ends in the path seperator character.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <exception cref="Exception"></exception>
    public static void ThrowOnInvalidPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new Exception("Path must have a non-whitespace value");
        if (path.Contains('\\')) throw new Exception("All paths must use forward slashes");
        if (path == "." || path == "..") throw new Exception("Relative paths are not allowed");
        if (path.EndsWith(IVirtualFs.PathSeparator)) throw new Exception("Paths cannot end in the path separator");
    }

    /// <summary>
    /// Matches glob patterns to aid with implementation of file/directory enumeration methods.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="pattern">The glob pattern to check against.</param>
    /// <returns>Whether the string is a match</returns>
    public static bool MatchGlob(string value, string pattern)
    {
        if (pattern == "*") return true;

        int pos = 0;
        while (pattern.Length != pos)
        {
            switch (pattern[pos])
            {
                case '?': break;

                case '*':
                    for (int i = value.Length; i >= pos; i--)
                    {
                        if (MatchGlob(value[i..], pattern[(pos + 1)..]))
                            return true;
                    }
                    return false;
                default:
                    if (value.Length == pos || char.ToUpper(pattern[pos]) != char.ToUpper(value[pos]))
                        return false;
                    break;
            }

            pos++;
        }

        return value.Length == pos;
    }

    private static List<string> TraverseNamesUpwards(IVirtualFsObject vfso)
    {
        var parts = new List<string>();
        if (vfso is null)
            throw new NullReferenceException(nameof(vfso));
        if (vfso is IVirtualFile file)
            parts.Add(file.Filename);
        else if (vfso is IVirtualFs dir)
            parts.Add(dir.DirectoryName);
        else throw new NotImplementedException("Only IVirtualFile and IVirtualFs are implemented");

        for (var parentDir = vfso.ContainingDirectory; parentDir != null; parentDir = parentDir.ContainingDirectory)
            parts.Insert(0, parentDir.DirectoryName);
        return parts;
    }
}
