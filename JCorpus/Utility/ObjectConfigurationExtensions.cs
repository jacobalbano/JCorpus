using Common.Addins;
using Common.Configuration;
using JCorpus.DI;
using JCorpus.Jobs;
using JCorpus.Web.Resources;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Utility;

internal static class ObjectConfigurationExtensions
{
    public static T Create<T>(this ObjectConfiguration<T> config, IServiceProvider provider) where T : IAddin
        => provider.GetRequiredService<ObjectConfigurator>().Create(config);
}
