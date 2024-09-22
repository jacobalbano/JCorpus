using Common.Addins;
using Common.IO;
using Utility.IO;

namespace Narou;

public class UiProvider : IUiProviderFor<Narou>
{
    public IVirtualFs Root => UiRoot;

    private static IVirtualFs UiRoot { get; } = new EmbeddedResourceFilesystem(typeof(Constants).Assembly)
        .Directory("UI")
        .AsReadOnly();
}
