using LionFire.Behaviors.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{

    public abstract class PollingCondition : PolledBehavior, ICondition
    {
        //#region ConditionState

        //public bool? ConditionState
        //{
        //    get { return conditionState; }
        //    protected set
        //    {
        //        if (conditionState == value) return;
        //        conditionState = value;

        //        var ev = ConditionStateChangedFor;
        //        if (ev != null) ev(this);
        //    }
        //} private bool? conditionState;

        //public event Action<ICondition> ConditionStateChangedFor;

        //#endregion

        
    }

}
