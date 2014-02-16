#if NoDeps
//#define TRACE_COROUTINE_FINISHED
//#define COROUTINES_RETURN_TIMESPANS
//#define COROUTINES_RETURN_LONG_STOPWATCH_TICKS
//#define CoroutineEnumeratorHasRecurranceParameters
//#define TRACE_QUEUE_EACH_LOOP

using LionFire.Meta;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Coroutines
{

    //public class CoroutineResult
    //{
    //    public float WaitTime
    //    {
    //        get { return WaitTimeTicks / (float) TimeSpan.TicksPerSecond; }
    //        set { WaitTimeTicks = (long)(value * (float) TimeSpan.TicksPerSecond); }
    //    }
    //    public double WaitTimeDouble
    //    {
    //        get { return WaitTimeTicks / (double)TimeSpan.TicksPerSecond; }
    //        set { WaitTimeTicks =(long)(value * (double)TimeSpan.TicksPerSecond); }
    //    }
    //    public long WaitTimeTicks;
    //}
    //public class WaitSeconds
    //{
    //    public float WaitTime
    //    {
    //        get { return WaitTimeTicks / (float)TimeSpan.TicksPerSecond; }
    //        set { WaitTimeTicks = (long)(value * (float)TimeSpan.TicksPerSecond); }
    //    }
    //    public double WaitTimeDouble
    //    {
    //        get { return WaitTimeTicks / (double)TimeSpan.TicksPerSecond; }
    //        set { WaitTimeTicks = (long)(value * (double)TimeSpan.TicksPerSecond); }
    //    }
    //    public long WaitTimeTicks;
    //}

#if FUTURE // Coroutine Priorities
    public class CoroutinePriorityInfo
    {
    #region Priority

        [SetOnce]
        public int Priority
        {
            get { return priority; }
            set
            {
                if (priority == value) return;
                if (priority != int.MinValue) throw new AlreadySetException();
                priority = value;
            }
        } private int priority = int.MinValue;

    #endregion

        public long WarnDeferTime { get; set; }
        public long MaxDeferTime { get; set; }
    }
#endif

#if FUTURE
    public interface ICoroutineHost
    {
        bool SingleThread { get; set; }
        void EnqueueCoroutine(IEnumerator enumerator);
        void RunCoroutine(IEnumerator enumerator);
    }
#endif

    public class DuplicatesAllowedComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x < y)
                return -1;
            else
                return 1;
        }
    }

    public class CoroutineHost : IDisposable, IHasCoroutineHost
    {
        #region IHasCoroutineHost Implementation

        CoroutineHost IHasCoroutineHost.CoroutineHost { get { return this; } }

        #endregion

        #region Parameters

        public bool SingleThread { get { return singleThread; } set { singleThread = value; } }
        private bool singleThread = true;

        #endregion

        #region (Private) Queues

        int queueIndex = 0; // REVIEW: make volatile?
        private ConcurrentQueue<CoroutineState> queue = new ConcurrentQueue<CoroutineState>();
        private ConcurrentQueue<CoroutineState> queue2 = new ConcurrentQueue<CoroutineState>();

        /// <summary>
        /// These all get run on every Execute
        /// </summary>
        private SortedList<int, CoroutineState> orderedCoroutines = new SortedList<int,CoroutineState>(new DuplicatesAllowedComparer());
        private IList<int> orderedCoroutinesKeys;
        private IList<CoroutineState> orderedCoroutinesValues;
        private object orderedCoroutinesLock = new object();

        #endregion

        #region (Private) Thread safety

        [Conditional("SanityChecks")]
        private void VerifyThread()
        {
#if SanityChecks
            if (ManagedThreadId == -1) { ManagedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId; }
            else { if (ManagedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId) { throw new InvalidOperationException("Execute methods can only be called from one thread"); } }
#endif
        }

#if SanityChecks
        private int ManagedThreadId = -1;
#endif

        #endregion

        #region (Private) State

        long _lastTickCount = 0;

        #endregion

        #region Construction / Disposal

        public CoroutineHost()
        {
            _lastTickCount = Stopwatch.GetTimestamp();

            orderedCoroutinesKeys = orderedCoroutines.Keys;
            orderedCoroutinesValues = orderedCoroutines.Values;
        }

        #region IDisposable

        public void Dispose()
        {
            IsLoopEnabled = false;
        }

        #endregion

        #endregion

#if CoroutineEnumeratorHasRecurranceParameters
        private static RecurranceParameters TryFindRP(object obj)
        {
            RecurranceParameters recurranceParameters = null;
            //if (RecurranceParameters == null)
            {
                var hasRP = obj as IHasRecurranceParameters;
                recurranceParameters = hasRP.RecurranceParameters;

                if (recurranceParameters == null)
                {
                    recurranceParameters = obj as RecurranceParameters;
                }
            }
            return recurranceParameters;
        }
#endif

        #region (Public) Start Methods

        //        public void EnqueueCoroutine(IEnumerator coroutine, RecurranceParameters rp = null)
        //        {            
        //#if CoroutineEnumeratorHasRecurranceParameters
        //            if (rp == null)
        //            {
        //                rp = TryFindRP(coroutine);
        //            }
        //#endif

        //            var cs = new CoroutineState(f, rp);

        //            var activeQueue = queueIndex == 0 ? queue : queue2;
        //            activeQueue.Enqueue(cs);
        //        }

        internal CoroutineState EnqueueCoroutine(IEnumerator coroutine, RecurranceParameters rp = null, long initialDelay = -1)
        {
#if CoroutineEnumeratorHasRecurranceParameters
            if (rp == null)
            {
                rp = TryFindRP(coroutine);
            }
#endif

            var cs = new CoroutineState(coroutine, rp);
            if (initialDelay != -1)
            {
                cs.WaitTimeTicks = initialDelay;
            }

            // REVIEW - queue in activeQueue and maybe run it too soon, or inactiveQueue, and run too late?

            if (rp != null && rp.Order != 0)
            {
                lock (orderedCoroutinesLock)
                {
                    orderedCoroutines.Add(rp.Order, cs);
                }
            }
            else
            {
                var inactiveQueue = queueIndex != 0 ? queue : queue2;
                inactiveQueue.Enqueue(cs);
            }
            return cs;
        }

        /// <summary>
        /// Run coroutine immediately
        /// </summary>
        /// <param name="result"></param>
        /// <param name="rp"></param>
        internal CoroutineState RunCoroutine(IEnumerator result, RecurranceParameters rp = null)
        {
            if (SingleThread) VerifyThread();
            try
            {
                if (!result.MoveNext()) { return null; }
            }
            catch (Exception ex)
            {
                l.Error("[COROUTINE] " + ex);
                return null;
            }

            long newWaitTicks = -1;

#if COROUTINES_RETURN_TIMESPANS
            if (result.Current is TimeSpan)
            {
                TimeSpan ts = (TimeSpan)result.Current;
                newWaitTicks = ts.ToStopwatchTicks();
            }
#endif
#if COROUTINES_RETURN_LONG_STOPWATCH_TICKS
            else if (result.Current is long)
            {
                newWaitTicks = (long)result.Current;
            }
#endif

            if (
#if COROUTINES_RETURN_LONG_STOPWATCH_TICKS || COROUTINES_RETURN_TIMESPANS
                newWaitTicks == -1 &&
#endif
rp != null)
            {
                newWaitTicks = rp.StopwatchInterval;
            }

            var coroutineState = new CoroutineState(result, rp);
            if (newWaitTicks != -1)
            {
                coroutineState.WaitTimeTicks = newWaitTicks;
            }
            else
            {
                if (coroutineState.ResetWaitTimeTicks != null)
                {
                    coroutineState.ResetWaitTimeTicks(coroutineState);
                }
                //else
                //{
                //    l.Warn("s.ResetWaitTimeTicks == null");
                //}
            }

            // TOTHREADSAFETY - inactiveQueue may get switched to active queue here.  Should
            // still be ok if it is a ConcurrentQueue.  
            // But it will falsely register that a certain amount of time has passed.
            var inactiveQueue = queueIndex != 0 ? queue : queue2;
            inactiveQueue.Enqueue(coroutineState);
            return coroutineState;
        }

        #endregion

        private bool ExecuteCS(CoroutineState coroutineState, long elapsedTicks)
        {
            try
            {
                if (coroutineState.WaitTimeTicks > elapsedTicks)
                {
                    coroutineState.WaitTimeTicks -= elapsedTicks;
                    //inactiveQueue.Enqueue(coroutineState);
                    return true;
                }
                else
                {
                    IEnumerator enumerator = coroutineState.Enumerator;

                    try
                    {
                        if (enumerator != null && enumerator.MoveNext())
                        {
#if COROUTINES_RETURN_TIMESPANS
                                if (enumerator.Current is TimeSpan)
                                {
                                    TimeSpan ts = (TimeSpan)enumerator.Current;
                                    coroutineState.WaitTimeTicks = ts.ToStopwatchTicks();
                                }
                                else 
#endif
#if COROUTINES_RETURN_LONG_STOPWATCH_TICKS
                                    if(enumerator.Current is long)
                                {
                                    coroutineState.WaitTimeTicks = (long)enumerator.Current ;
                                }
                               else
#endif

                            {
                                if (coroutineState.ResetWaitTimeTicks != null)
                                {
                                    coroutineState.ResetWaitTimeTicks(coroutineState);
                                }
                                //else
                                //{
                                //    l.Warn("s.ResetWaitTimeTicks == null");
                                //}
                            }
                            //inactiveQueue.Enqueue(coroutineState);
                            return true;
                        }
#if TRACE_COROUTINE_FINISHED
                            else
                            {
                                l.Warn("[coroutine] UNTESTED finished: " + coroutineState.ToString());
                            }
#endif
                    }
                    catch (Exception ex)
                    {
                        l.Error("[COROUTINE] " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                l.Error(ex.ToString());
            }
            return false;
        }

        public bool Execute(int maxItems = -1)
        {
            if (SingleThread) VerifyThread();

            long lastTickCount = _lastTickCount;

            long currentTicks = _lastTickCount = Stopwatch.GetTimestamp();
            long elapsedTicks = currentTicks - lastTickCount;

            var currentTime = TimeSpan.FromTicks(currentTicks);

            var activeQueue = queueIndex == 0 ? queue : queue2;
            var inactiveQueue = queueIndex != 0 ? queue : queue2;

            int startTotal = activeQueue.Count + inactiveQueue.Count;
            int activeQueueCount = activeQueue.Count;
            CoroutineState coroutineState;

            IEnumerator<KeyValuePair<int, CoroutineState>> e = orderedCoroutines.GetEnumerator();
            int orderedIndex ;
            for (orderedIndex = 0; orderedIndex < orderedCoroutines.Count; orderedIndex++)
            {
                //var kvp = orderedCoroutines.GetB
            }
            lock (orderedCoroutines)
            {
                while (e.MoveNext() && e.Current.Key < 0)
                {
                    coroutineState = e.Current.Value;
                    ExecuteCS(coroutineState, elapsedTicks);
                }
            }

            while (maxItems != 0)
            {
                if (!activeQueue.TryDequeue(out coroutineState))
                {
                    //if (activeQueueCount > 0 && )
                    //{
                    //    throw new UnreachableCodeException();
                    //}
                    break;
                }

                if (ExecuteCS(coroutineState, elapsedTicks)) { inactiveQueue.Enqueue(coroutineState); }

                
                
              
                
                
                if (maxItems > 0) maxItems--;
                
                //if (q.Count == 0) break;
            }

#if TRACE_QUEUE_EACH_LOOP
            if (activeQueue.Count > 0)
            {
                //l.Debug("Active queue: ");
                foreach (var item in activeQueue)
                {
                    l.Debug(" + " + item.ToString());
                }
            }
            if (inactiveQueue.Count > 0)
            {
                //l.Debug("Inactive queue: ");
                foreach (var item in inactiveQueue)
                {
                    l.Debug(" - " + item.ToString());
                }
            }
#endif

            int endTotal = activeQueue.Count + inactiveQueue.Count;

            if (endTotal == 0 && startTotal > 0)
            {
                l.Info("All coroutines finished");
            }

            if (activeQueue.Count == 0)
            {
                queueIndex ^= 1;
                //q = queueIndex == 0 ? queue : queue2;
                return true;
            }
            return false;
        }

        #region Loop (Optional.  If unused, call Execute yourself)

        #region IsLoopEnabled

        public bool IsLoopEnabled
        {
            get { return isLoopEnabled; }
            set
            {
                if (isLoopEnabled == value) return;
                isLoopEnabled = value;
                if (isLoopEnabled)
                {
                    if (LoopInterval == TimeSpan.Zero)
                    {
                        loopThread = new Thread(new ThreadStart(() => _ExecuteLoop()));
                    }
                    else
                    {
                        loopThread = new Thread(new ThreadStart(() => _ExecuteLoopInterval()));
                    }
                    loopThread.IsBackground = IsLoopBackgroundThread;
                    loopThread.Priority = LoopThreadPriority;
                    loopThread.Start();

                    //ThreadPool.QueueUserWorkItem(x => CHLoop());
                }
                else
                {
                    ManagedThreadId = -1;
                }
            }
        } private bool isLoopEnabled;
        Thread loopThread;

        #endregion

        #region IsLoopBackgroundThread

        public bool IsLoopBackgroundThread
        {
            get { return isLoopBackgroundThread; }
            set { isLoopBackgroundThread = value; }
        } private bool isLoopBackgroundThread = true;

        #endregion

        #region LoopThreadPriority

        public ThreadPriority LoopThreadPriority
        {
            get { return loopThreadPriority; }
            set { loopThreadPriority = value; }
        } private ThreadPriority loopThreadPriority = ThreadPriority.Normal;

        #endregion

        #region LoopInterval

        /// <summary>
        /// If changed from zero, change will not take effect until IsLoopEnabled is set to false and true again.
        /// </summary>
        public TimeSpan LoopInterval
        {
            get { return loopInterval; }
            set
            {
                loopInterval = value;
            }
        } private TimeSpan loopInterval = TimeSpan.Zero;

        #endregion

        /// <summary>
        /// Invoke repeatedly as fast as possible, yielding after each loop.
        /// </summary>
        private void _ExecuteLoop()
        {
            //l.Debug("Coroutine loop starting.");
            while (isLoopEnabled)
            {
                l.Trace("Coroutine loop ");
                Execute();
#if NET35
                Thread.Sleep(0);
#else
                Thread.Yield();
#endif
            }
            ManagedThreadId = -1;
            //l.Debug("Coroutine loop exiting.");
        }

        private void _ExecuteLoopInterval()
        {
            l.Trace("Coroutine interval loop starting.");
            while (isLoopEnabled)
            {
                DateTime start = DateTime.UtcNow;
                Execute();
                DateTime finish = DateTime.UtcNow;

                // REVIEW - not precise, but good enough?
                TimeSpan sleepTime = loopInterval - (finish - start);
                double ratio = sleepTime.Ticks / (double)loopInterval.Ticks;
                double tolerance = 0.05;
                if (sleepTime > TimeSpan.Zero)
                {
                    if ((ratio > 1 + tolerance || ratio < 1 - tolerance))
                    {
                        l.Trace("Coroutine interval loop sleeping for " + sleepTime.TotalMilliseconds + "ms");
                    }
                    Thread.Sleep(sleepTime);
                }
                else
                {
#if NET35
                    Thread.Sleep(0);
#else
                    Thread.Yield();
#endif
                }

            }
            ManagedThreadId = -1;
            l.Trace("Coroutine loop exiting.");
        }

        #endregion


        #region Misc

        private static ILogger l = Log.Get();

        #endregion
    }



}
#endif