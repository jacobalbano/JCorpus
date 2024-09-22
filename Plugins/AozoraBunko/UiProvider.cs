using Common.IO;
using Common.Addins;
using Utility.IO;

namespace AozoraBunko;

public class UiProvider : IUiProviderFor<AozoraBunko>
{
    public IVirtualFs Root => UiRoot;

    private static IVirtualFs UiRoot { get; } = new EmbeddedResourceFilesystem(typeof(Constants).Assembly)
        .Directory("UI")
        .AsReadOnly();
}