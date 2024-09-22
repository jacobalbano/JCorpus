using Common.DI;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.StaticWebsites;
using GenHTTP.Modules.Webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Web;

[AutoDiscover(AutoDiscoverOptions.Singleton)]
internal class WebserviceHost
{
    public void Start() => host.Start();
    public void Stop() => host.Stop();

    public WebserviceHost(IEnumerable<IWebResource> resources, GenHTTPLogging logging, Website website)
    {
        var api = Layout.Create();
        foreach (var resource in resources)
            api.AddService(resource.GetType().Name, resource);

        host = Host.Create()
#if DEBUG
            .Development(true)
#endif
            .Defaults()
            .Port(4649)
            .Companion(logging)
            .Add(CorsPolicy.Permissive())
            .Handler(Layout.Create()
                .Add(StaticWebsite.From(website.Build()))
                .Add("api", api));
    }

    private readonly IServerHost host;
}
