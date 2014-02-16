#if NoDeps
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire
{
    public static class TimeSpanExtensions
    {
        public static long ToStopwatchTicks(this TimeSpan timeSpan)
        {
            // DOUBLECAST
            return (long)(timeSpan.TotalSeconds * System.Diagnostics.Stopwatch.Frequency);
        }

        public static TimeSpan StopwatchTicksToTimeSpan(this long stopwatchTicks)
        {
            // DOUBLECAST
            return TimeSpan.FromSeconds(stopwatchTicks / (double)System.Diagnostics.Stopwatch.Frequency); 
        }
    }
}
#endif