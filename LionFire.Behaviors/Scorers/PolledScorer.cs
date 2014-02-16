using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public abstract class PolledScorer<T> : Scorer<T>, IPolledScorer
        where T : IBehavior, new()
    {
#if NO_COROUTINES
        public virtual IPollingProvider PollingProvider { get { var hasPP = Context as IHasPollingProvider; return hasPP != null ? hasPP.PollingProvider : null; } }
#endif

        #region Score

        public abstract void UpdateScore();

        #endregion

        #region IsMonitoringScore

        protected override void OnIsMonitoringScoreChangedTo(bool isMonitoringScore)
        {
            IsScorePollerAttached = isMonitoringScore;
        }

        private bool IsScorePollerAttached
        {
            get { return scorePoller != null; }
            set
            {
                if (IsScorePollerAttached == value) return;
                if (value)
                {
                    scorePoller = new ScorePoller(this);
                    scorePoller.IsRunning = true;
                }
                else
                {
                    scorePoller.IsRunning = false;
                    scorePoller = null;
                }
            }
        }
        private ScorePoller scorePoller;

        #endregion

        //#region ScoreChangedForTo Event

        //protected override void RaiseScoreChanged()
        //{
        //    var ev = scoreChangedForTo;
        //    if (ev != null) ev(this, Score);
        //}

        //public override event Action<IScorer, float> ScoreChangedForTo
        //{
        //    add
        //    {
        //        if (scoreChangedForTo == null && scorePoller == null)
        //        {
        //            IsScorePollerAttached = true;
        //        }
        //        scoreChangedForTo += value;
        //    }
        //    remove
        //    {
        //        scoreChangedForTo -= value;
        //        if (scoreChangedForTo == null && scorePoller != null)
        //        {
        //            IsScorePollerAttached = false;
        //        }
        //    }
        //}
        //private event Action<IScorer, float> scoreChangedForTo;

        //#endregion

    }


}
