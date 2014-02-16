using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public abstract class RunOnceCondition : Condition
    {
        #region RunOnceAndExit

        public bool RunOnceAndExit
        {
            get { return runOnceAndExit; }
            set { runOnceAndExit = value; }
        } private bool runOnceAndExit;

        #endregion

        //protected override BehaviorStatus OnStart()
        //{
        //    if (Context == null) Fault(ContextMissingMessage);

        //    if (Context.IsAlive) Fail(EntityDiedMessage);

        //    if (RunOnceAndExit)
        //    {
        //        return BehaviorStatus.Succeeded;
        //    }
        //    else
        //    {
        //        Context.FlagsChangedForFromTo += Context_FlagsChangedForFromTo;
        //        return BehaviorStatus.Running;
        //    }
        //}

    }

    public abstract class Condition : Behavior
    {
        

        public override string StatusWord
        {
            get
            {
                if (IsMonitoring)
                {
                    switch (Status)
                    {
                        //case BehaviorStatus.Uninitialized:
                        //    break;
                        //case BehaviorStatus.Initialized:
                        //    break;
                        //case BehaviorStatus.Running:
                        //    break;
                        case BehaviorStatus.Failed:
                            return "Watching";
                        case BehaviorStatus.Succeeded:
                            return "Active";
                        //case BehaviorStatus.Disposed:
                        //    break;
                        //case BehaviorStatus.Suspended:
                        //    break;
                        default:
                            break;
                    }
                }
                return base.StatusWord;
            }
        }

        
    }

    
}
