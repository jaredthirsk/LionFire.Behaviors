using System;

namespace LionFire.Behaviors
{
    public abstract class Behavior : BehaviorBase
    {
        #region StatusChangedForTo

        protected sealed override void RaiseStatusChanged(BehaviorStatus oldStatus)
        {
            var ev = StatusChangedForFromTo;
            if (ev != null) ev(this, oldStatus, Status);
        }

        public override event Action<IBehavior, BehaviorStatus, BehaviorStatus> StatusChangedForFromTo;

        #endregion

        
    }
}
