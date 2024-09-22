using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using JCorpus.Web.Transit.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Web.Resources;

internal class Job : IWebResource
{
    [ResourceMethod]
    public IEnumerable<string> Jobs() => jobs.Keys;

    [ResourceMethod(RequestMethod.GET, ":job")]
    public TransitConfigSchema Schema(string job)
    {
        if (!jobs.TryGetValue(job, out var jobType))
            throw new ProviderException(ResponseStatus.NotFound, "Not found");

        return TransitConfigSchema.GetDescriptionIfConfigurable(jobType);
    }

    private readonly Dictionary<string, Type> jobs = jobTypes
        .ToDictionary(x => x.Name);

    private static readonly IReadOnlyList<Type> jobTypes = new[]
    {
        typeof(Accelerate),
        typeof(Analyze),
        typeof(Extract),
        typeof(Search),
    };
}
