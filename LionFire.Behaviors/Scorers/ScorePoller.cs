using LionFire.Coroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public class ScorePoller : PollerBase
    {
        IPolledScorer polled { get { return PollTarget as IPolledScorer; } set { PollTarget = value; } }

        #region Construction

        public ScorePoller(IPolledScorer polled, RecurranceParameters recurranceParameters = null)
        {
            if (recurranceParameters == null)
            {
                var hasRP = polled as IHasScorerRecurranceParameters;
                if (hasRP != null)
                {
                    recurranceParameters = hasRP.ScorerRecurranceParameters;
                }
            }
            SetPolledAndRP(polled, recurranceParameters);
        }

        #endregion

        protected override void DoPoll()
        {
            try
            {
                polled.UpdateScore();
            }
            catch (Exception ex)
            {
                l.Error("ScorePoller - UpdateScore threw exception: " + ex.ToString());
            }
        }

        #region Misc

        private static ILogger l = Log.Get();

        #endregion
    }
    
}
