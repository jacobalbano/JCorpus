using Common.DI;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.StaticWebsites;
using GenHTTP.Modules.StaticWebsites.Provider;
using JCorpus.Implementation.IO.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility.IO;

namespace JCorpus.Web;

[AutoDiscover(AutoDiscoverOptions.Transient)]
internal class Website
{
    public Website(IPersistentDirectory<Website> workingDir)
    {
        this.workingDir = workingDir;
    }

    public IResourceTree Build()
    {
#if DEBUG
        // TODO: don't hardcode this
        var asm = AppDomain.CurrentDomain.BaseDirectory;
        var solution = new DirectoryInfo(asm).Parent.Parent.Parent;
        var website = Path.Combine(solution.FullName, "Content", "Website");
        var path = WindowsPathUtility.MakeDirectoryPath(website);
        return new VirtualFsResourceTree(new VirtualFs(path));
#else
        var fs = new EmbeddedResourceFilesystem(typeof(Website).Assembly)
            .Directory("Content/Website")
            .AsReadOnly();

        var tree = new VirtualFsResourceTree(fs);
#endif
    }

    private readonly IPersistentDirectory<Website> workingDir;
}
