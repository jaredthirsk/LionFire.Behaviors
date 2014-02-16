using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors.Behaviors
{
    public abstract class PolledBehavior : BehaviorBase, IPolledStatus
    {
#if NO_COROUTINES
        public virtual IPollingProvider PollingProvider { get { var hasPP = Context as IHasPollingProvider; return hasPP != null ? hasPP.PollingProvider : null; } }
#endif

        #region Status

        public abstract void UpdateStatus();

        #endregion

        protected override BehaviorStatus OnStart()
        {
            IsStatusPollerAttached = true;
            return BehaviorStatus.Running;
        }
        protected override void OnFinished()
        {
            IsStatusPollerAttached = false;
            base.OnFinished();
        }

        #region Status Poller

        private bool IsStatusPollerAttached
        {
            get { return statusPoller != null; }
            set
            {
                if (IsStatusPollerAttached == value) return;
                if (value)
                {
                    statusPoller = new StatusPoller(this);
                    statusPoller.IsRunning = true;
                }
                else
                {
                    //statusPoller.IsRunning = false;
                    statusPoller.Succeed();
                    statusPoller = null;
                }
            }
        }
        private StatusPoller statusPoller;

        #endregion

        #region ScoreChangedForTo Event

        protected sealed override void RaiseStatusChanged(BehaviorStatus oldStatus)
        {
            var ev = statusChangedForFromTo;
            if (ev != null) ev(this, oldStatus, Status);
        }

        public override event Action<IBehavior, BehaviorStatus, BehaviorStatus> StatusChangedForFromTo
        {
            add
            {
                //if (statusChangedForFromTo == null && statusPoller == null)
                //{
                //    IsStatusPollerAttached = true;
                //}
                statusChangedForFromTo += value;
            }
            remove
            {
                statusChangedForFromTo -= value;
                //if (statusChangedForFromTo == null && statusPoller != null)
                //{
                //    IsStatusPollerAttached = false;
                //}
            }
        }
        private event Action<IBehavior, BehaviorStatus, BehaviorStatus> statusChangedForFromTo;

        #endregion
    }
}
