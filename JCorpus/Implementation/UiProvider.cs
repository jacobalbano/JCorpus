using Common.Addins;
using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.IO;

namespace JCorpus.Implementation;

public class UiProvider : IUiProviderFor<JCorpus>
{
    public IVirtualFs Root => UiRoot;

    private static IVirtualFs UiRoot { get; } = new EmbeddedResourceFilesystem(typeof(JCorpus).Assembly)
        .Directory("Implementation/WebUi")
        .AsReadOnly();
}
