using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Utility;

internal static class PeriodicJob
{
    public static void Run(Duration duration, Func<Task> action)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(duration.ToTimeSpan());
                await action();
            }
        });
    }
}
