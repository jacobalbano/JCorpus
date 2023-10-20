using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DI;
using Utility;

namespace JCorpus.DI;

[AutoDiscover(AutoDiscoverOptions.Scoped)]
internal class ConfigRepository
{
    [AutoDiscover(AutoDiscoverOptions.Scoped, ImplementationFor = typeof(IConfigProvider<>))]
    public class ConfigProviderImpl<T> : IConfigProvider<T>
    {
        public ConfigProviderImpl(ConfigRepository repository)
        {
            this.repository = repository;
        }

        public T? Get() => repository.Get<T>();

        private readonly ConfigRepository repository;
    }

    public void Provide(object? config)
    {
        if (config == null)
            return;

        var type = Nullable.GetUnderlyingType(config.GetType()) ?? config.GetType();
        configs.Add(type, config);
    }

    public T? Get<T>()
    {
        if (!configs.TryGetValue(typeof(T), out var result) || result is not T typedResult)
            return default;

        return typedResult;
    }

    public void Freeze()
    {
        frozen = true;
    }

    private bool frozen = false;
    private Dictionary<Type, object> configs = new();
}
