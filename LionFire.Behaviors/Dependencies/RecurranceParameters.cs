#if NoDeps
using LionFire.Meta;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Coroutines
{
    
    // RENAME to CoroutineParameters
    public class RecurranceParameters
    {
        #region Interval

        /// <summary>
        /// Do not run until this goes down to 0
        /// </summary>
        //[SetOnce]
        public TimeSpan Interval
        {
            get
            {
                return StopwatchInterval.StopwatchTicksToTimeSpan();
                //return TimeSpan.FromSeconds(StopwatchInterval / (double)System.Diagnostics.Stopwatch.Frequency); // DOUBLECAST
            }
            set
            {
                StopwatchInterval = value.ToStopwatchTicks();
                //StopwatchInterval = (long)(value.TotalSeconds * System.Diagnostics.Stopwatch.Frequency); // DOUBLECAST
            }
        }
        //private TimeSpan interval = TimeSpan.MinValue;

        public long StopwatchInterval { get; set; }

        #endregion

        #region StartImmediately

        [DefaultValue(false)]
        public bool StartImmediately
        {
            get { return startImmediately; }
            set { startImmediately = value; }
        } private bool startImmediately = false;

        #endregion

#if MAX_RECURRANCES
        public int MaxRecurrances { get; set; }
#endif

        [DefaultValue(0)]
        public int Order { get; set; }

    }

    

}
#endif