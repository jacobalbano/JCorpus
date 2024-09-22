using Common.IO;
using NodaTime;
using OS = System.IO;

namespace JCorpus.Implementation.IO.Filesystem;

internal partial class VirtualFs
{
    private class VirtualFile : IVirtualFile
    {
        public FilePath Filename { get; }
        public IVirtualFs ContainingDirectory { get; }

        public VirtualFile(IVirtualFs directory, FilePath filename)
        {
            ContainingDirectory = directory;
            Filename = filename;
        }

        public bool IsReadOnly => Exists && (ContainingDirectory.IsReadOnly || OS.File.GetAttributes(this.GetFullyQualifiedPath())
            .HasFlag(FileAttributes.ReadOnly));

        public bool Exists => OS.File.Exists(this.GetFullyQualifiedPath());

        public Stream Open(FileMode mode)
        {
            ThrowIfReadOnly(this, mode);
            var access = (mode == FileMode.Open || IsReadOnly) ? FileAccess.Read : FileAccess.ReadWrite;
            return OS.File.Open(this.GetFullyQualifiedPath(), mode, access, FileShare.Read);
        }

        public void Delete()
        {
            ThrowIfReadOnly(this);
            OS.File.Delete(this.GetFullyQualifiedPath());
        }

        private static void ThrowIfReadOnly(IVirtualFile file)
        {
            if (file.IsReadOnly)
                throw new ReadOnlyFilesystemException();
        }

        private static void ThrowIfReadOnly(IVirtualFile file, FileMode mode)
        {
            if (!file.IsReadOnly) return;

            switch (mode)
            {
                case FileMode.CreateNew:
                case FileMode.Create:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                case FileMode.Append:
                    throw new ReadOnlyFilesystemException();
            }
        }
    }
}
