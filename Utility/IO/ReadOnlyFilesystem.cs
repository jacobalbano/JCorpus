using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.IO;

public class ReadOnlyFilesystem : IVirtualFs
{
    public bool IsReadOnly => true;
    public bool Exists => dir.Exists;

    public DirectoryPath DirectoryName => dir.DirectoryName;
    public IVirtualFs ContainingDirectory => dir.ContainingDirectory.AsReadOnly();
    public IVirtualFile File(FilePath path) => dir.File(path);
    public IVirtualFs Directory(DirectoryPath path) => dir.Directory(path).AsReadOnly();

    public void Create() => throw new ReadOnlyFilesystemException();
    public void Delete() => throw new ReadOnlyFilesystemException();

    public IEnumerable<FilePath> EnumerateFiles(string pattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        => dir.EnumerateFiles(pattern, searchOption);

    public IEnumerable<DirectoryPath> EnumerateDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        => dir.EnumerateDirectories(searchOption);


    public ReadOnlyFilesystem(IVirtualFs dir) => this.dir = dir;
    private readonly IVirtualFs dir;
}

public static class ReadOnlyFsExt
{
    public static IVirtualFs AsReadOnly(this IVirtualFs dir) => new ReadOnlyFilesystem(dir);
}
