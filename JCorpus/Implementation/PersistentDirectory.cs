using Common.DI;
using Common.IO;
using JCorpus.Implementation.IO.Filesystem;
using JCorpus2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace JCorpus.Implementation;

[AutoDiscover(AutoDiscoverOptions.Transient, ImplementationFor = typeof(IPersistentDirectory<>))]
internal class PersistentDirectory<T> : VirtualFs, IPersistentDirectory<T>
{
    public PersistentDirectory(ProgramConstants constants) : base(DirectoryPath.Combine(constants.WorkingDirectory, typeof(T).Name))
    {
        Create();
    }
}
