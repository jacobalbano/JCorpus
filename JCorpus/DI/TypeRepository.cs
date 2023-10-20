using Common;
using Common.DI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.DI;

internal interface ITypeRepository<T>
{
    public bool TryGetTypeByName(string name, [MaybeNullWhen(false)] out Type stageImplementationType);
}

[AutoDiscover(AutoDiscoverOptions.Singleton, ImplementationFor = typeof(ITypeRepository<>))]
internal class TypeRepository<T> : ITypeRepository<T>
{
    public TypeRepository(IEnumerable<T> implementations)
    {
        types = implementations
            .Select(x => x.GetType())
            .ToDictionary(x => x.Name);
    }

    public bool TryGetTypeByName(string name, [MaybeNullWhen(false)] out Type stageImplementationType)
    {
        return types.TryGetValue(name, out stageImplementationType);
    }

    public IEnumerable<string> GetTypenames() => types.Keys;

    private readonly Dictionary<string, Type> types;
}
