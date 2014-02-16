using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LionFire.Behaviors
{
#if NO_COROUTINES
    public interface IPollingProvider
    {
        void Register(IPoller poller);
        void Unregister(IPoller poller);
    }
#endif
}
