#if NO_COROUTINES
//using LionFire.Behaviors;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace LionFire.Coroutines
//{
//    public class CoroutineHostPollingProvider : IPollingProvider
//    {        
//        #region CoroutineHost

//        public CoroutineHost CoroutineHost
//        {
//            get { return coroutineHost; }
//            set
//            {
//                if (coroutineHost == value) return;
//                if (coroutineHost != default(CoroutineHost)) throw new AlreadySetException();
//                coroutineHost = value;
//            }
//        } private CoroutineHost coroutineHost;

//        #endregion

//        #region Construction

//        public CoroutineHostPollingProvider()
//        {
//        }
//        public CoroutineHostPollingProvider(CoroutineHost coroutineHost)
//        {
//            this.CoroutineHost = coroutineHost;
//        }

//        #endregion
        
//        public void Register(IPoller poller)
//        {
//#if NoBuiltinPollerCoroutines
//            coroutineHost.StartCoroutine(new PollerCoroutine(poller));
//#endif
//            coroutineHost.EnqueueCoroutine(poller, poller.RecurranceParameters);
//        }

//        public void Unregister(IPoller poller)
//        {
//            // No-op.  Will deregister the next time it gets executed.
//        }
//    }
//}
#endif