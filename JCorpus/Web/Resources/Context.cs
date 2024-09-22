using Common;
using Common.IO;
using Common.Addins.Search;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.FileSystem;
using GenHTTP.Modules.Webservices;
using JCorpus.Implementation.IO.Filesystem;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenHTTP.Modules.IO.Streaming;

namespace JCorpus.Web.Resources;

internal class Context : IWebResource
{
    [ResourceMethod(RequestMethod.GET, ":corpusWorkId/:reference")]
    public IResponse Get(string corpusWorkId, string reference, IRequest request)
    {
        // TODO: support returning multiple results in the API
        var evidence = evidenceProviders.SelectMany(x => x.GetEvidence(corpusWorkId, reference))
            .FirstOrDefault() ?? throw new ProviderException(ResponseStatus.NotFound, "Not found");

        var ms = new MemoryStream(evidence.Data);
        return request.Respond()
            .Content(ms, () => ms.CalculateChecksumAsync())
            .Type(FlexibleContentType.Get(ContentType.ImageJpg)) // TODO: don't hardcode this 
            .Build();
    }

    public Context(IEnumerable<IContextProvider> evidenceProviders)
    {
        this.evidenceProviders = evidenceProviders;
    }

    private readonly IEnumerable<IContextProvider> evidenceProviders;
}
