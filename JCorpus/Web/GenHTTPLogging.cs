using Common.DI;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Providers.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JCorpus.Web;

[AutoDiscover(AutoDiscoverOptions.Transient)]
internal class GenHTTPLogging : IServerCompanion
{
    public void OnRequestHandled(IRequest request, IResponse response)
    {
        var body = string.Empty;
        if (request.Method.KnownMethod == RequestMethod.POST && request.ContentType == ContentType.ApplicationJson)
        {
            request.Content?.Seek(0, SeekOrigin.Begin);
            body = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<JsonDocument>(request.Content),
                new JsonSerializerOptions() { WriteIndented = true }
            );
            request.Content?.Seek(0, SeekOrigin.Begin);
        }

        logger.LogDebug("[{method} {phrase}] {path} {query} {body}", request.Method.RawMethod, response.Status.Phrase, request.Target.Path, request.Query, body);
    }

    public void OnServerError(ServerErrorScope scope, Exception error)
    {
        logger.LogDebug(error, "Server error in {scope}", scope);
    }

    public GenHTTPLogging(ILogger<GenHTTPLogging> logger)
    {
        this.logger = logger;
    }

    private readonly ILogger logger;
}
