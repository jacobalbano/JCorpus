using Common.IO;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace JCorpus.Web;

internal class VirtualFsResourceTree : IResourceTree
{
    public DateTime? Modified => null;

    public IVirtualFs Dir { get; }
    public VirtualFsResourceTree(IVirtualFs Dir) => this.Dir = Dir;

    public IAsyncEnumerable<IResourceNode> GetNodes()
        => Dir.EnumerateDirectories()
            .Select(x => new Node(this, Dir.Directory(x)))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<IResource> GetResources()
        => Dir.EnumerateFiles()
            .Select(x => new Resource(Dir.File(x)))
            .ToAsyncEnumerable();

    public ValueTask<IResourceNode> TryGetNodeAsync(string name)
    {
        var dir = Dir.Directory(name);
        if (!dir.Exists) return new();
        return new(new Node(this, dir));
    }

    public ValueTask<IResource> TryGetResourceAsync(string name)
    {
        var file = Dir.File(name);
        if (!file.Exists) return new();
        return new(new Resource(file));
    }

    private class Node : VirtualFsResourceTree, IResourceNode
    {
        public IResourceContainer Parent { get; }
        public string Name => Dir.DirectoryName;

        public Node(IResourceContainer parent, IVirtualFs dir) : base(dir)
            => Parent = parent;
    }

    private class Resource : IResource
    {
        public IVirtualFile File { get; }

        public Resource(IVirtualFile file)
        {
            if (!file.Exists)
                throw new FileNotFoundException("File does not exist", file.GetFullyQualifiedPath());

            File = file;

            using var stream = TryGetStream();
            Length = (ulong)stream.Length;
        }

        public string Name => File.Filename;
        public DateTime? Modified => null;
        public FlexibleContentType ContentType => null;

        public ulong? Length { get; }

        public async ValueTask<ulong> CalculateChecksumAsync()
        {
            if (checksum is null)
            {
                using var stream = TryGetStream();
                checksum = await stream.CalculateChecksumAsync() ?? throw new InvalidOperationException("Unable to calculate checksum of assembly resource");
            }

            return checksum.Value;
        }

        public ValueTask<Stream> GetContentAsync() => new(TryGetStream());

        public async ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            using var content = TryGetStream();
            await content.CopyPooledAsync(target, bufferSize);
        }

        private Stream TryGetStream() => File.Open(FileMode.Open);

        private ulong? checksum;
    }
}
