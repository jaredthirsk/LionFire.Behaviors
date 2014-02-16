#if NO_COROUTINES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    
    public interface IHasPollingProvider
    {
        IPollingProvider PollingProvider { get; }
    }
}
#endif