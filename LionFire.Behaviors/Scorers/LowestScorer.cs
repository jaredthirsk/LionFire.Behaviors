using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors.Scorers
{
    public abstract  class LowestScorer<T> : Scorer<T>
        where T : IBehavior, new()
    {
        #region Construction

        public LowestScorer() { }
        public LowestScorer(Func<IBehavior> nodeFactory)
            : base(nodeFactory)
        {
        }

        #endregion

    }
}
