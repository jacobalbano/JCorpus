using Common.DI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.DI;

[AutoDiscover(AutoDiscoverOptions.Transient)]
internal interface IFactory<T>
{
    T Create();
    T Create(params object[] args);
}

[AutoDiscover(AutoDiscoverOptions.Transient, ImplementationFor = typeof(IFactory<>))]
internal class FactoryImpl<T> : IFactory<T>
{
    T IFactory<T>.Create()
    {
        return services.GetService<T>();
    }

    T IFactory<T>.Create(params object[] args)
    {
        return ActivatorUtilities.CreateInstance<T>(services, args);
    }

    public FactoryImpl(IServiceProvider services)
    {
        this.services = services;
    }

    private readonly IServiceProvider services;
}
