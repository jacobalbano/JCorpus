using Common;
using Common.Addins;
using Common.Addins.Accelerate;
using Common.Addins.Analyze;
using Common.Addins.Extract;
using Common.Addins.Search;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using JCorpus.DI;
using JCorpus.Web.Transit.Schema;

namespace JCorpus.Web.Resources;

internal class Addin : IWebResource
{
    [ResourceMethod]
    public IEnumerable<string> Categories() => repos.Keys;

    [ResourceMethod(RequestMethod.GET, ":category")]
    public IEnumerable<AddinKey> Keys(string category)
    {
        if (!repos.TryGetValue(category, out var repo))
            throw new ProviderException(ResponseStatus.NotFound, "Not found");

        return repo.GetTypeKeys();
    }

    [ResourceMethod(RequestMethod.GET, ":category/:plugin")]
    public IEnumerable<string> Typenames(string category, string plugin)
    {
        if (!repos.TryGetValue(category, out var repo))
            throw new ProviderException(ResponseStatus.NotFound, "Not found");

        return repo.GetTypeKeys()
            .Where(x => x.PluginName == plugin)
            .Select(x => x.AddinTypeName);
    }

    [ResourceMethod(RequestMethod.GET, ":category/:plugin/:implementation")]
    public TransitConfigSchema Schema(string category, string plugin, string implementation)
    {
        if (!repos.TryGetValue(category, out var repo) || !repo.TryGetTypeByKey(new(plugin, implementation), out var type))
            throw new ProviderException(ResponseStatus.NotFound, "Not found");

        return TransitConfigSchema.GetDescriptionIfConfigurable(type);
    }

    private readonly Dictionary<string, IAddinRepository> repos = new();

    public Addin(IServiceProvider services)
    {
        foreach (var type in repositoryTypes)
        {
            var repoFor = type.GetGenericArguments().First();
            repos[repoFor.Name] = (IAddinRepository) services.GetService(type);
        }
    }

    private readonly IReadOnlyList<Type> repositoryTypes = new[]
    {
        typeof(IAddinRepository<ISearch>),
        typeof(IAddinRepository<IAnalysis>),
        typeof(IAddinRepository<IAccelerator>),
        typeof(IAddinRepository<ICorpusWorkSource>),
        typeof(IAddinRepository<ICorpusWorkExtractor>),
    };
}
