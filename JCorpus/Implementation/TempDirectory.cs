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

[AutoDiscover(AutoDiscoverOptions.Transient, ImplementationFor = typeof(ITempDirectory))]
internal class TempDirectory : VirtualFs, ITempDirectory
{
    public TempDirectory(ProgramConstants constants) : base(DirectoryPath.Combine(constants.WorkingDirectory, "Temp", Path.GetRandomFileName()))
    {
        Create();
    }

    public void Dispose()
    {
        if (disposed) throw new ObjectDisposedException(nameof(TempDirectory));
        Delete();
        disposed = true;
    }

    private bool disposed = false;
}
