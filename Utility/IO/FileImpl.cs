using Common.IO;

namespace Utility.IO;

public partial class EmbeddedResourceFilesystem
{
    private class FileImpl : IVirtualFile
    {
        public FilePath Filename { get; }
        public IVirtualFs? ContainingDirectory => parent;
        public bool IsReadOnly => true;

        public bool Exists
        {
            get
            {
                var path = this.GetLocallyQualifiedPath();
                return parent.resourceEntries.Keys.Where(x => !x.EndsWith(IVirtualFs.PathSeparator))
                    .Where(x => x.TrimEnd(IVirtualFs.PathSeparator) == path)
                    .Any();
            }
        }

        public FileImpl(EmbeddedResourceFilesystem parent, FilePath filePath)
        {
            this.parent = parent;
            Filename = filePath;
        }

        public Stream Open(FileMode mode)
        {
            if (mode != FileMode.Open)
                throw new InvalidOperationException();

            if (!parent.resourceEntries.TryGetValue(this.GetLocallyQualifiedPath(), out var resourceKey))
                throw new Exception("Invalid resource key");

            return parent.asm.GetManifestResourceStream(resourceKey);
        }

        public void Delete() => throw new InvalidOperationException();

        private readonly EmbeddedResourceFilesystem parent;
    }
}
