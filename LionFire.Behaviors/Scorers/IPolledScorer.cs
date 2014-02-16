using LionFire.Coroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public interface IHasScorerRecurranceParameters
    {
        RecurranceParameters ScorerRecurranceParameters { get; }
    }

    public interface IPolledScorer : IScorer
#if NO_COROUTINES
        , IHasPollingProvider
#endif
    {
        void UpdateScore();        
    }
}
