#if NoDeps
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Coroutines
{
    public interface IHasRecurranceParameters
    {
        /// <summary>
        /// Default parameters to use for a poller
        /// </summary>
        RecurranceParameters RecurranceParameters { get; }
    }
}
#endif