using Common.DI;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace JCorpus.DI;

internal class SystemClockImpl : ISystemClock
{
    public ZonedDateTime ToLocalTime(Instant instant)
        => instant.InZone(DateTimeZoneProviders.Bcl.GetSystemDefault());

    public Instant GetCurrentInstant()
        => SystemClock.Instance.GetCurrentInstant();
}
