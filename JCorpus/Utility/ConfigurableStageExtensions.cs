using JCorpus.DI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Utility;

internal static class ConfigurableStageExtensions
{
    public static T Create<T>(this ConfigurableStage<T> stage, IServiceProvider provider)
    {
        return provider.GetRequiredService<PipelineStageFactory>()
            .Create(stage);
    }
}
