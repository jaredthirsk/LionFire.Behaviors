using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public abstract class ConstScorer<T> : Scorer<T>
        where T : IBehavior, new()
    {
        #region Construction

        public ConstScorer(float val, Func<IBehavior> childFactory=null)
            : base(childFactory)
        {
            this.Score = val;
        }

        #endregion

    }
}
