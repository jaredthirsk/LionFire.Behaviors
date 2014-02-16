using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public class RecurringState
    {
        /// <summary>
        /// How long 
        /// </summary>
        public long DeferTime = 0;

        /// <summary>
        /// Do not run until this goes down to 0
        /// </summary>
        public long IntervalRemaining;
    }
}
