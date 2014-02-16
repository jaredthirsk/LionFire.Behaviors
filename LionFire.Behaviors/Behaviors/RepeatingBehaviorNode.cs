// OLD

//#define LASTRUN
////using LionFire.Coroutines;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace LionFire.Behaviors
//{
//    public class RecurringBehaviorNode : Behavior, IRecurring, IHasCoroutine
////#if LASTRUN
////        , INotifyPropertyChanged
////#endif
//    {

//        //public RecurranceParameters TimingParameters { get; set; }
//        //public RecurringState TimingState { get; set; }

//        //public void Start()
//        //{
//        //    this.CoroutineHost.StartCoroutine(Enumerator);
//        //}

//        //public void Stop()
//        //{
//        //}

////        public IEnumerator Enumerator
////        {
////            get
////            {
////                while (true)
////                {          
////#if LASTRUN
////                    LastRun = Stopwatch.GetTimestamp();
////#endif
////                    var result = Tick();
////                    if (object.ReferenceEquals(Finished, result)) yield break;
////                    yield return result;
////                }
////            }
////        }

//        //private static readonly object Finished = new object();

//        //protected virtual object Tick()
//        //{
//        //    return Finished;
//        //}

////        #region Last Run
////#if LASTRUN
////        #region LastRun

////        public long LastRun
////        {
////            get { return lastRun; }
////            set
////            {
////                if (lastRun == value) return;
////                lastRun = value;
////                OnPropertyChanged("LastRun");
////            }
////        } private long lastRun;

////        #endregion
////#endif
////        #endregion
   
//        //#region IHasCoroutine Implementation

//        //public CoroutineHost CoroutineHost
//        //{
//        //    get { return CoroutineHostProvider.GetCoroutineHost(this); }
//        //}

//        //public virtual ICoroutineHostProvider CoroutineHostProvider
//        //{
//        //    get
//        //    {
//        //        return DefaultCoroutineHostProvider ?? Singleton<DefaultCoroutineHostProvider>.Instance;
//        //    }
//        //}

//        ///// <summary>
//        ///// If null, use Singleton&lt;DefaultCoroutineHostProvider&gt;.Instance
//        ///// </summary>
//        //public static ICoroutineHostProvider DefaultCoroutineHostProvider;

//        //#endregion


//        //#region Misc

//        //private static ILogger l = Log.Get();
		
//        //#endregion

//    }
//}
