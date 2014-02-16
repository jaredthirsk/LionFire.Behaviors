using LionFire.Coroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{

    public abstract class RecurringTask : Behavior, IHasRecurranceParameters, IDecorated
    {
        #region Parameters

        public RecurranceParameters RecurranceParameters
        {
            get;
            set;
        }

        #endregion

        #region Construction

        public RecurringTask()
        {
            
        }

        #endregion


        #region Start Poller

        private bool IsStartPollerAttached
        {
            get { return startPoller != null; }
            set
            {
                if (IsStartPollerAttached == value) return;
                if (value)
                {
                    startPoller = new OldPollingRepeater(this);
                    startPoller.IsRunning = true;
                }
                else
                {
                    startPoller.IsRunning = false;
                    startPoller = null;
                }
            }
        }

        public IBehavior Decorator { get { return StartPoller; } }
        public OldPollingRepeater StartPoller { get { return startPoller; } }
        private OldPollingRepeater startPoller;

        #endregion

        #region Transitions

        //protected override BehaviorStatus OnStart()
        //{
        //    IsStartPollerAttached = true;
        //    return BehaviorStatus.Running;
        //}
        //protected override void OnCanceling()
        //{
        //    IsStartPollerAttached = false;
        //    base.OnCanceling();
        //}

        //protected override void OnDeinitializing()
        //{
        //    startPoller = null;
        //    base.OnDeinitializing();
        //}

        #endregion

    }
}
