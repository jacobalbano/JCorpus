using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.IO.Zip;

public partial class ZipFilesystem : IVirtualFs
{
    public DirectoryPath DirectoryName { get; }

    public IVirtualFs ContainingDirectory { get; }

    public bool IsReadOnly => archive.Mode != ZipArchiveMode.Read;

    public bool Exists
    {
        get
        {
            var path = this.GetLocallyQualifiedPath();
            return archive.Entries.Any(x => x.FullName == path);
        }
    }

    public ZipFilesystem(ZipArchive archive)
    {
        DirectoryName = "ZIP";
        this.archive = archive;
    }

    public IEnumerable<DirectoryPath> EnumerateDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return GetSearchPaths(searchOption)
            .Where(x => x.EndsWith(IVirtualFs.PathSeparator))
            .Select(x => new DirectoryPath(x.TrimEnd(IVirtualFs.PathSeparator)));
    }

    public IEnumerable<FilePath> EnumerateFiles(string pattern = IVirtualFs.MatchAllFilesPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return GetSearchPaths(searchOption)
            .Where(x => !x.EndsWith(IVirtualFs.PathSeparator))
            .Where(x => MatchGlob(x, pattern))
            .Select(x => new FilePath(x));
    }

    public IVirtualFs Directory(DirectoryPath path)
    {
        ZipFilesystem result = this;
        foreach (var part in PathUtility.GetPathParts(path))
            result = new ZipFilesystem(result, part);

        return result;
    }

    public IVirtualFile File(FilePath path)
    {
        ZipFilesystem parent = this;
        foreach (var part in PathUtility.GetPathParts(path))
            parent = (ZipFilesystem) parent.Directory(part);

        return new FileImpl(parent, PathUtility.GetUnqualifiedFilePath(path));
    }

    public void Create()
    {
        if (!Exists) archive.CreateEntry(this.GetLocallyQualifiedPath() + '/');
    }

    public void Delete()
    {
        var path = this.GetLocallyQualifiedPath() + '/';
        var entries = archive.Entries.Where(x => x.FullName.StartsWith(path))
            .ToList();

        foreach (var entry in entries)
            entry.Delete();
    }

    private ZipFilesystem(ZipFilesystem parentDir, DirectoryPath directory) : this(parentDir.archive)
    {
        ContainingDirectory = parentDir;
        DirectoryName = directory;
    }

    private readonly ZipArchive archive;

    private IEnumerable<string> GetSearchPaths(SearchOption searchOption)
    {
        var searchPaths = archive.Entries
            .Select(x => x.FullName);

        if (ContainingDirectory != null)
        {
            var path = this.GetLocallyQualifiedPath().ToString();
            searchPaths = searchPaths
                .Where(x => x.StartsWith(path))
                .Select(x => x[(path.Length + 1)..])
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }

        if (searchOption == SearchOption.TopDirectoryOnly)
            searchPaths = searchPaths.Where(x => Math.Max(0, x.IndexOf('/')) < x.Length);

        return searchPaths;
    }

    private static bool MatchGlob(string value, string pattern)
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

    public override string ToString() => DirectoryName.ToString() + '/';
}
