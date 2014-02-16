using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public interface IScorer : IBehavior
    {
        float Score { get; }

        bool IsMonitoringScore { get; set; }
        event Action<IScorer, float> ScoreChangedForTo;

    }
}
