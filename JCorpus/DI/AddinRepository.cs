using Common;
using Common.Addins;
using Common.DI;
using JCorpus.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.DI;

internal interface IAddinRepository
{
    public bool TryGetTypeByKey(AddinKey key, out Type addinType);
    public IEnumerable<AddinKey> GetTypeKeys();
}

internal interface IAddinRepository<T> : IAddinRepository where T : IAddin
{
}

[AutoDiscover(AutoDiscoverOptions.Singleton, ImplementationFor = typeof(IAddinRepository<>))]
internal class AddinRepository<T> : IAddinRepository<T> where T : IAddin
{
    public AddinRepository(IEnumerable<T> implementations)
    {
        types = implementations
            .Select(x => x.GetType())
            .ToDictionary(x => MakeKey(x));
    }

    private static AddinKey MakeKey(Type type)
    {
        var plugin = PluginUtility.GetPluginFor(type);
        return new(plugin.PluginName, type.Name);
    }

    public bool TryGetTypeByKey(AddinKey key, out Type addinType)
    {
        return types.TryGetValue(key, out addinType);
    }

    public IEnumerable<AddinKey> GetTypeKeys() => types.Keys;

    private readonly Dictionary<AddinKey, Type> types;
}
