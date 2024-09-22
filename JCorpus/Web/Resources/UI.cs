using Common.Addins;
using Common.IO;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.StaticWebsites;
using GenHTTP.Modules.StaticWebsites.Provider;
using GenHTTP.Modules.Webservices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JCorpus.Web.Resources;

internal class UI : IWebResource
{
    [ResourceMethod]
    public IEnumerable<string> Categories() => roots.Keys;

    [ResourceMethod(RequestMethod.GET, ":pluginName")]
    public async Task<IHandlerBuilder> Get(string pluginName)
    {
        if (!roots.TryGetValue(pluginName, out var root))
            throw new ProviderException(ResponseStatus.NotFound, "Not found");
        return StaticWebsite.From(root);
    }

    public UI(IEnumerable<IUiProvider> uiProviders)
    {
        roots = uiProviders.ToDictionary(
            x => x.DirectoryName,
            x => new VirtualFsResourceTree(x.Root)
        );
    }

    private readonly IReadOnlyDictionary<string, VirtualFsResourceTree> roots;
}
