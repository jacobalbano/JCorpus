using Common.DI;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility;

[AutoDiscover(AutoDiscoverOptions.Singleton | AutoDiscoverOptions.Implementations)]
public interface ISystemClock : IClock
{
    string GetTimestampFilename() => ToLocalTime(GetCurrentInstant())
        .ToString("yyyy-MM-dd_HH_mm_ss", null);

    ZonedDateTime ToLocalTime(Instant instant);
}