#if NO_COROUTINES
//#define CreateDefaultPollingProvider
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public class Dependencies
    {
        #region PollingProvider

        public static IPollingProvider PollingProvider
        {
            get { return pollingProvider ?? DefaultPollingProvider; }
            set { pollingProvider = value; }
        } private static IPollingProvider pollingProvider;

        #endregion


        public static IPollingProvider DefaultPollingProvider { get { return defaultPollingProvider; } set { defaultPollingProvider = value; } }
        private static IPollingProvider defaultPollingProvider 
#if CreateDefaultPollingProvider
            = Singleton<TimerPollingProvider>.Instance // REVIEW
#endif
        ;

        public static void CreateDefaultPollingProviderIfNeeded()
        {
            if (defaultPollingProvider == null)
            {
                defaultPollingProvider = Singleton<TimerPollingProvider>.Instance;
            }
        }
    }
}
#endif