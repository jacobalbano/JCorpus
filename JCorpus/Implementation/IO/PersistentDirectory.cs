using Common.DI;
using Common.IO;
using JCorpus.Implementation.IO.Filesystem;
using JCorpus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.IO;

namespace JCorpus.Implementation.IO;

[AutoDiscover(AutoDiscoverOptions.Transient, ImplementationFor = typeof(IPersistentDirectory<>))]
internal class PersistentDirectory<T> : VirtualFs, IPersistentDirectory<T>
{
    public PersistentDirectory(ProgramConstants constants) : base(Path(constants))
    {
        Create();
    }

    private static DirectoryPath Path(ProgramConstants constants)
        => DirectoryPath.Combine(
            constants.WorkingDirectory,
            PluginUtility.GetPluginFor<T>().PluginName,
            typeof(T).Name
        );
}
