#define RequirePollTargetBehaviorContext
using LionFire.Coroutines;
//#define MAX_RECURRANCES
//#define CheckPollTargetForIPollingProvider
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public class PollerSettings
    {
        #region (Static) Defaults

        public static RecurranceParameters DefaultRecurranceParameters = new RecurranceParameters()
        {
            Interval = TimeSpan.FromMilliseconds(1000)
        };

        #endregion
    }

    public abstract class PollerBase : Behavior, IPoller // Make this a Decorator?
    {
        #region Ontology

        public object PollTarget { get { return _pollTarget; }
             protected set
            {
                _pollTarget = value;
                IBehavior b = value as IBehavior;

                if (b != null)
                {
                    if(Context==null)
                    {
                    this.Context = b.Context;
                    }
                }
            }
        }
        private object _pollTarget;

        #endregion

        //protected override bool RemoveChildrenOnFinish
        //{
        //    get
        //    {
        //        return false;
        //    }
        //}

        #region Construction

        /// <summary>
        /// Construct a Poller
        /// </summary>
        /// <param name="polled">If this implements IHasRecurranceParameters, IHasRecurranceParameters.RecurranceParameters will be used if none is specified as a parameter</param>
        /// <param name="recurranceParameters"></param>
        public PollerBase()
        {
        }

        protected void SetPolledAndRP(object polled, RecurranceParameters recurranceParameters)
        {
            this.PollTarget = polled; // Also sets context

            if (recurranceParameters != null)
            {
                RecurranceParameters = recurranceParameters;
            }
            else
            {
                IHasRecurranceParameters hasRP = polled as IHasRecurranceParameters;
                if (hasRP != null)
                {
                    RecurranceParameters = hasRP.RecurranceParameters;
                }
            }
        }

        #endregion

        #region RecurranceParameters

        public RecurranceParameters RecurranceParameters
        {
            get
            {
                if (recurranceParameters != null) return recurranceParameters;

                IHasRecurranceParameters hasRP = PollTarget as IHasRecurranceParameters;
                if (hasRP != null && hasRP.RecurranceParameters != null) return hasRP.RecurranceParameters;

                return PollerSettings.DefaultRecurranceParameters;
                //return recurranceParameters ?? ; 
            }
            set
            {
                if (recurranceParameters == value) return;

                bool wasEnabled = IsRunning;

                if (wasEnabled)
                {
                    IsRunning = false;
                }

                if (recurranceParameters != default(RecurranceParameters)) throw new AlreadySetException("");
                recurranceParameters = value;

                if (wasEnabled)
                {
                    IsRunning = true;
                }
            }
        } private RecurranceParameters recurranceParameters;

        #endregion

        #region RecurranceState

#if MAX_RECURRANCES
        private int recurrancesRemaining = 0;
#endif

        #endregion

#if POLLER_COROUTINES
        public bool Poll()
#else
        public bool Poll()
#endif
        {
            if (!IsRunning) return false;

            // THREADSAFETY - Note: Poll may be executed after IsRunning is set to false elsewhere
            DoPoll();

#if MAX_RECURRANCES
            if (RecurranceParameters.MaxRecurrances > 0)
            {
                recurrancesRemaining--;
                if (recurrancesRemaining <= 0)
                {
                    IsRunning = false;
                }
            }
#endif
            return IsRunning;
        }

        /// <summary>
        /// Set IsRunning = false to stop polling
        /// </summary>
        protected abstract void DoPoll();

#if !NO_COROUTINES
        Coroutine coroutine;
#endif

        private void TryGetContextFromPollTarget()
        {
            IBehavior b = PollTarget as IBehavior;

            if (b.Context == null)
            {
                b.TrySetContextFromParent();
            }
            this.Context = b.Context;
        }

        protected override BehaviorStatus OnStart()
        {            
#if RequirePollTargetBehaviorContext            
            if (Context == null)
            {
                TryGetContextFromPollTarget();
            }            
            //if (Context == null) throw new Exception("RequirePollTargetBehaviorContext");
            if (Context == null) throw new Exception("RequirePollerBehaviorContext");
#endif

#if MAX_RECURRANCES
            recurrancesRemaining = RecurranceParameters.MaxRecurrances;
#endif

#if NO_COROUTINES
            var hasPP = pollTarget as IHasPollingProvider;
            if (hasPP != null && hasPP.PollingProvider != null)
            {
                hasPP.PollingProvider.Register(this);
            }
            else
            {
#if CheckPollTargetForIPollingProvider
                var ipp = pollTarget as IPollingProvider;
                if (ipp != null)
                {
                    ipp.Register(this);
                }
                else
#endif
                {
                    var pp = Dependencies.PollingProvider;
                    if (pp == null) throw new InvalidOperationException(NoPollerMessage);
                    pp.Register(this);
                }
            }
#else
            // Can't do run here because it might fail before we can set it to running :-/.  But pollers
            // shouldn't have to run immediately.
            coroutine = Coroutine.Start(this,this.Context, this.RecurranceParameters);
            coroutine.SetName(this.GetType().Name);
#endif
            //if (RecurranceParameters.StartImmediately) { DoPoll(); }
            return BehaviorStatus.Running;
        }

#if NO_COROUTINES
        private const string NoPollerMessage = "No polling provider set.  Set one in LionFire.Behavior.Dependencies.PollingProvider, or implement IHasPollingProvider on the polled object.";
#endif

        protected override void OnCanceling(string msg = null)
        {
            #if NO_COROUTINES
            var pp = Dependencies.PollingProvider;
            if (pp == null) throw new InvalidOperationException(NoPollerMessage);
            pp.Unregister(this);
#else
            coroutine.Dispose();
#endif
        }

#if !NoPollerCoroutines
        #region IEnumerator

        object IEnumerator.Current
        {
            get { return current; }
        } private object current;

        bool IEnumerator.MoveNext()
        {
            current = null;
            lock (statusLock)
            {
                if (IsRunning)
                {
                    current = Poll();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        void IEnumerator.Reset()
        {
        }

        #endregion
#endif

    }
}
