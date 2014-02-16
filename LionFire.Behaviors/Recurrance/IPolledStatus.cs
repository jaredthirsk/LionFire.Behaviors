using LionFire.Coroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{


    public interface IHasStatusRecurranceParameters : IHasRecurranceParameters
    {
        //RecurranceParameters StatusRecurranceParameters { get; } OLD
        RecurranceParameters RecurranceParameters { get;  }
    }

    public interface IPolledStatus : IBehavior
#if NO_COROUTINES
        , IHasPollingProvider
#endif
    {
        void UpdateStatus();
    }
}
