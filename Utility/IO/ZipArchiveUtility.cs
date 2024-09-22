using Common.IO;
using Common.IO.Zip;
using System.IO.Compression;

namespace Utility.IO;

public static class ZipArchiveUtility
{
    public static void ExtractTo(this ZipArchive archive, IVirtualFs directory, bool overwrite = false)
    {
        var fs = new ZipFilesystem(archive);
        foreach (var path in fs.EnumerateFiles(searchOption: SearchOption.AllDirectories))
            fs.File(path).CopyTo(directory.Directory(path.GetDirectoryPath()), overwrite);
    }
}
