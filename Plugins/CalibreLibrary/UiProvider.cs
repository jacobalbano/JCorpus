using Common.IO;
using Common.Addins;
using Utility.IO;

namespace CalibreLibrary;

public class UiProvider : IUiProviderFor<CalibreLibrary>
{
    public IVirtualFs Root => UiRoot;

    private static IVirtualFs UiRoot { get; } = new EmbeddedResourceFilesystem(typeof(Constants).Assembly)
        .Directory("UI")
        .AsReadOnly();
}