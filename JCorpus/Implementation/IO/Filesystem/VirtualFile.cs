using Common.IO;
using OS = System.IO;

namespace JCorpus.Implementation.IO.Filesystem;

internal partial class VirtualFs
{
    private class VirtualFile : IVirtualFile
    {
        public FilePath Filename { get; }
        public IVirtualFs? ContainingDirectory { get; }

        public VirtualFile(IVirtualFs directory, FilePath filename)
        {
            ContainingDirectory = directory;
            Filename = filename;
        }

        public bool IsReadOnly => OS.File.GetAttributes(this.GetFullyQualifiedPath())
            .HasFlag(FileAttributes.ReadOnly);

        public bool Exists => OS.File.Exists(this.GetFullyQualifiedPath());

        public Stream Open(FileMode mode)
        {
            return OS.File.Open(this.GetFullyQualifiedPath(), mode);
        }

        public void Delete()
        {
            OS.File.Delete(this.GetFullyQualifiedPath());
        }
    }
}
