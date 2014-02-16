#if NO_COROUTINES
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LionFire.Behaviors
{
    public class TimerPollingProvider : IPollingProvider
    {
        //ConcurrentDictionary<long, Timer> timersByDuration = new ConcurrentDictionary<long, Timer>(); // TOAOT - 
        ConcurrentDictionary<IPoller, Timer> pollers = new ConcurrentDictionary<IPoller, Timer>();

        #region Configuration

        #region ExceptionBehavior

        public PollingExceptionBehavior ExceptionBehavior
        {
            get { return exceptionBehavior; }
            set { exceptionBehavior = value; }
        } private PollingExceptionBehavior exceptionBehavior = PollingExceptionBehavior.Disable | PollingExceptionBehavior.WrapAndRethrow;

        #endregion

        #endregion
        
        #region Construction

        public TimerPollingProvider()
        {
#if AOT
            throw new NotSupportedException("TODO: Dictionary IComparer in ctor");
#endif
        }

        #endregion

        #region IPollingProvider Implementation

        public void Register(IPoller poller)
        {
            var rp = poller.RecurranceParameters;
            if (rp == null) { throw new ArgumentNullException("IPoller instance does not have RecurranceParameters set"); }

            int milliseconds = (int)rp.Interval.TotalMilliseconds; // DOUBLECAST

            Timer timer = new Timer(OnTimerElapsed, poller, milliseconds, milliseconds);

            if (!pollers.TryAdd(poller, timer)) throw new DuplicateNotAllowedException();

            //poller.RecurranceParameters.Interval
        }

        public void Unregister(IPoller poller)
        {
            Timer timer;
            if (pollers.TryRemove(poller, out timer))
            {
                timer.Dispose();
            }
        }

        #endregion

        #region (Private) Event handling

        private void OnTimerElapsed(object state)
        {
            IPoller poller = (IPoller)state;
            bool result;
            try
            {
#if POLLER_COROUTINES
                result = poller.MoveNext();

#else // Poller Method, slightly more efficient
                result = poller.Poll();
#endif
            }
            catch(Exception ex)
            {
                if(((ExceptionBehavior & PollingExceptionBehavior.WrapAndRethrow) != PollingExceptionBehavior.None))
                {
                    throw new Exception("Poller threw exception", ex);
                }
                if (((ExceptionBehavior & PollingExceptionBehavior.Disable) != PollingExceptionBehavior.None))
                {
                    result = false;
                }
                else
                {
                    result = true;
                }                
            }
            if (!result)
            {
                Unregister(poller);
            }
        }

        #endregion

    }
}
#endif