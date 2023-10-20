using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utility.IO;

public partial class EmbeddedResourceFilesystem : IVirtualFs
{
    public EmbeddedResourceFilesystem(Assembly asm)
    {
        this.asm = asm;
        var DirectoryName = asm.GetName().Name!;
        resourceEntries = asm.GetManifestResourceNames()
            .ToDictionary(x => GetFileNameFromResourceName(x[(DirectoryName.ToString().Length + 1)..]));
    }

    private EmbeddedResourceFilesystem(EmbeddedResourceFilesystem parentDir, DirectoryPath directory, IReadOnlyDictionary<string, string> resourceEntries)
    {
        ContainingDirectory = parentDir;
        DirectoryName = directory;
        this.resourceEntries = resourceEntries;
        asm = parentDir.asm;
    }

    public DirectoryPath DirectoryName { get; }

    public IVirtualFs? ContainingDirectory { get; }

    public bool IsReadOnly => true;

    public bool Exists => resourceEntries.ContainsKey(this.GetLocallyQualifiedPath().ToString());

    public void Create() => throw new InvalidOperationException();
    public void Delete() => throw new InvalidOperationException();

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
            .Where(x => PathUtility.MatchGlob(x, pattern))
            .Select(x => new FilePath(x));
    }

    public IVirtualFs Directory(DirectoryPath path)
    {
        var result = this;
        foreach (var part in PathUtility.GetPathParts(path))
            result = new EmbeddedResourceFilesystem(result, part, resourceEntries);

        return result;
    }

    public IVirtualFile File(FilePath path)
    {
        var parent = this;
        foreach (var part in PathUtility.GetPathParts(path))
            parent = (EmbeddedResourceFilesystem)parent.Directory(part);

        return new FileImpl(parent, PathUtility.GetUnqualifiedFilePath(path));
    }

    private IEnumerable<string> GetSearchPaths(SearchOption searchOption)
    {
        var searchPaths = resourceEntries.Keys.AsEnumerable();

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

    // from https://stackoverflow.com/questions/32173741/how-to-get-filename-of-embedded-resource-of-an-exe
    private static string GetFileNameFromResourceName(string resourceName)
    {
        // NOTE: this code assumes that all of the file names have exactly one
        // extension separator (i.e. "dot"/"period" character). I.e. all file names
        // do have an extension, and no file name has more than one extension.
        // Directory names may have periods in them, as the compiler will escape these
        // by putting an underscore character after them (a single character
        // after any contiguous sequence of dots). IMPORTANT: the escaping
        // is not very sophisticated; do not create folder names with leading
        // underscores, otherwise the dot separating that folder from the previous
        // one will appear to be just an escaped dot!

        var sb = new StringBuilder();
        bool escapeDot = false, haveExtension = false;

        for (int i = resourceName.Length - 1; i >= 0; i--)
        {
            if (resourceName[i] == '_')
            {
                escapeDot = true;
                continue;
            }

            if (resourceName[i] == '.')
            {
                if (!escapeDot)
                {
                    if (haveExtension)
                    {
                        sb.Append(IVirtualFs.PathSeparator);
                        continue;
                    }
                    haveExtension = true;
                }
            }
            else
            {
                escapeDot = false;
            }

            sb.Append(resourceName[i]);
        }

        return new string(sb.ToString().Reverse().ToArray());
    }

    private readonly IReadOnlyDictionary<string, string> resourceEntries;
    private readonly Assembly asm;
}
