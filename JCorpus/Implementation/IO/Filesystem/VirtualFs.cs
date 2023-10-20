using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.IO;
using OS = System.IO;

namespace JCorpus.Implementation.IO.Filesystem;

internal partial class VirtualFs : IVirtualFs
{
    public IVirtualFs? ContainingDirectory { get; }

    public DirectoryPath DirectoryName { get; }

    public bool IsReadOnly => false;

    public bool Exists => OS.Directory.Exists(this.GetFullyQualifiedPath());

    public VirtualFs(DirectoryPath rootDirectory)
    {
        DirectoryName = rootDirectory;
    }

    public IEnumerable<DirectoryPath> EnumerateDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        var fullPath = this.GetFullyQualifiedPath().ToString();
        return OS.Directory.EnumerateDirectories(fullPath, IVirtualFs.MatchAllFilesPattern, searchOption)
            .Select(x => x[fullPath.Length..])
            .Select(WindowsPathUtility.MakeDirectoryPath);

    }

    public IEnumerable<FilePath> EnumerateFiles(string pattern = IVirtualFs.MatchAllFilesPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        var fullPath = this.GetFullyQualifiedPath().ToString();
        return OS.Directory.EnumerateFiles(fullPath, pattern, searchOption)
            .Select(x => x[fullPath.Length..])
            .Select(WindowsPathUtility.MakeFilePath);
    }

    public IVirtualFs Directory(DirectoryPath path)
    {
        VirtualFs result = this;
        foreach (var part in PathUtility.GetPathParts(path))
            result = new VirtualFs(result, part);

        return result;
    }

    public IVirtualFile File(FilePath path)
    {
        VirtualFs parent = this;
        foreach (var part in PathUtility.GetPathParts(path))
            parent = (VirtualFs) parent.Directory(part);

        return new VirtualFile(parent, PathUtility.GetUnqualifiedFilePath(path));
    }

    public void Create()
    {
        OS.Directory.CreateDirectory(this.GetFullyQualifiedPath());
    }

    public void Delete()
    {
        OS.Directory.Delete(this.GetFullyQualifiedPath(), true);
    }

    private VirtualFs(VirtualFs parentDir, DirectoryPath directory) : this(directory)
    {
        ContainingDirectory = parentDir;
    }
}
