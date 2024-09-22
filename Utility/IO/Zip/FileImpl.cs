using System.IO;
using System.IO.Compression;

namespace Common.IO.Zip;

public partial class ZipFilesystem
{
    private class FileImpl : IVirtualFile
    {
        public FilePath Filename { get; }
        public IVirtualFs ContainingDirectory => parent;

        public FileImpl(ZipFilesystem directory, FilePath filename)
        {
            parent = directory;
            Filename = filename;
        }

        public bool IsReadOnly => false;

        public bool Exists => GetEntry() != null;

        public Stream Open(FileMode mode)
        {
            var entry = GetEntry();
            switch (mode)
            {
                case FileMode.CreateNew:
                    if (entry != null) throw new IOException("Entry already exists");
                    goto case FileMode.Create;
                case FileMode.Create:
                    if (entry != null) Delete();
                    return parent.archive.CreateEntry(this.GetLocallyQualifiedPath()).Open();
                case FileMode.OpenOrCreate:
                    if (entry == null) goto case FileMode.Create;
                    goto case FileMode.Open;
                case FileMode.Open:
                    if (entry == null) throw new FileNotFoundException("No matching entry");
                    return entry.Open();
                case FileMode.Truncate:
                case FileMode.Append:
                default:
                    throw new NotSupportedException();
            }
        }

        public void Delete() => GetEntry()?.Delete();

        private readonly ZipFilesystem parent;

        public override string ToString() => Filename.ToString();

        private ZipArchiveEntry GetEntry()
        {
            var path = this.GetLocallyQualifiedPath();
            return parent.archive.Entries.Where(x => !x.FullName.EndsWith(IVirtualFs.PathSeparator))
                .Where(x => x.FullName.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
        }
    }
}
