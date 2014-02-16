#if TRACE
#define TRACE_RANDOM
#endif
using LionFire.Coroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    /// <summary>
    /// Can be injected in a Sequence to be used as an unpredictable sleep
    /// </summary>
    public class RandomCondition : PollingCondition, IPolledStatus
        , IHasStatusRecurranceParameters
    {
        public int ProbabilityPercent = 10;

        public override string StatusText
        {
            get
            {
                return lastN.ToString();
            }
        }

        private int lastN = -1;

        public override void UpdateStatus()
        {
            lastN = RandomProvider.ThreadRandom.Next(0, 100);
            RaiseStatusTextChanged();

#if TRACE_RANDOM
            l.Trace(()=>"RandomCondition UpdateStatus() " + lastN.ToString());
#endif

            if (lastN > 100 - ProbabilityPercent)
            {
                Succeed();
            }
        }

        public RecurranceParameters RecurranceParameters
        {
            get
            {
                return statusRecurranceParameters; // TODO - inherit from parent??
            }
            set
            {
                statusRecurranceParameters = value;
            }
        } private RecurranceParameters statusRecurranceParameters;

        #region Misc

        private static ILogger l = Log.Get();

        #endregion
    }

}
