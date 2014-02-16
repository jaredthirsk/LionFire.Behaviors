using LionFire.Coroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public class StatusPoller : PollerBase
    {
        IPolledStatus polled { get { return PollTarget as IPolledStatus; } set { PollTarget = value; } }
        object locker = new object();

        #region Construction

        public StatusPoller(IPolledStatus polled, RecurranceParameters recurranceParameters = null
            //, object context = null
            ) 
        {
            if (recurranceParameters == null)
            {
                var hasRP = polled as IHasStatusRecurranceParameters;
                if (hasRP != null)
                {
                    recurranceParameters = hasRP.RecurranceParameters;
                }
            }
            SetPolledAndRP(polled, recurranceParameters); 
        }

        #endregion

        protected override void DoPoll()
        {
            try
            {
                lock (locker)
                {
                    polled.UpdateStatus();

                    if (polled.Status != BehaviorStatus.Running)
                    {
                        IsRunning = false;
                    }
                }
            }
            catch (Exception ex)
            {
                l.Error("ScorePoller - UpdateScore threw exception: " + ex.ToString());
            }
        }

        #region Misc

        public override string ToString()
        {
            return "<" + base.ToString() + "-" + polled.ToString() + ">" ;
        }
        private static ILogger l = Log.Get();

        #endregion

        public new void Succeed()
        {
            base.Succeed();
        }
    }
}
