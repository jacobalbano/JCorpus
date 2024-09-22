using Common.DI;
using Common.IO;
using JCorpus.Implementation.IO.Filesystem;
using JCorpus.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.IO;

namespace JCorpus.Implementation.IO;

[AutoDiscover(AutoDiscoverOptions.Transient, ImplementationFor = typeof(IFileSystemAccess<>))]
internal class FilesystemAccess<T> : IFileSystemAccess<T>
{
    public IVirtualFs Access(DirectoryPath path)
    {
        var plugin = PluginUtility.GetPluginFor<T>().PluginName;
        logger.LogWarning("{plugin}/{type} accessed {path}", plugin, typeof(T).Name, path);
        return new VirtualFs(path).AsReadOnly();
    }

    public FilesystemAccess(ILogger<FilesystemAccess<T>> logger)
    {
        this.logger = logger;
    }

    private readonly ILogger logger;
}
