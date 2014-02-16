#define BehaviorPoller_SuspendAndResume
using LionFire.Coroutines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    /// <summary>
    /// Automatically stopped when the Behavior is stopped.
    /// Automatically suspended/resumed when the Behavior is suspended/resumed .
    /// </summary>
    public class BehaviorCoroutine : IPoller
    {
        #region Ontology

        IBehavior behavior;
        IEnumerator enumerator;

        #endregion

        #region Parameters

        public RecurranceParameters RecurranceParameters
        {
            get { return recurranceParameters; }
        } RecurranceParameters recurranceParameters;

        #endregion

        #region State

        Coroutine coroutine;

        #endregion

        #region Construction

        public BehaviorCoroutine(IBehavior behavior, IEnumerator enumerator, RecurranceParameters recurranceParameters = null)
        {
            if (behavior == null) throw new ArgumentNullException("behavior");
            if (enumerator == null) throw new ArgumentNullException("enumerator");
            this.behavior = behavior;
            this.enumerator = enumerator;
            this.recurranceParameters = recurranceParameters;

            #region this.coroutineHost = ...
            // OLD
            //public static CoroutineHost DefaultCoroutineHost;
            //this.coroutineHost = coroutineHost;

            //if (coroutineHost == null)
            //{
            //    IHasCoroutineHost hasCH = behavior.Context as IHasCoroutineHost;
            //    if (hasCH != null) coroutineHost = hasCH.CoroutineHost;
            //}

            //if (coroutineHost == null)
            //{
            //    coroutineHost = DefaultCoroutineHost;
            //}

            //if (coroutineHost == null)
            //{
            //    throw new ArgumentNullException("CoroutineHost must be set by behavior.Context (which implements IHasCoroutineHost), or in BehaviorCoroutine.DefaultCoroutineHost.");
            //}

            #endregion
            
            behavior.StatusChangedForFromTo += OnBehaviorStatusChanged;

            if (behavior.IsRunning)
            {
                StartCoroutine();
            }
        }

        #endregion

        #region Status Change Event Handler

        private void OnDisposed()
        {
            behavior.StatusChangedForFromTo -= OnBehaviorStatusChanged;
            behavior = null;
            StopCoroutine();            
        }

        void OnBehaviorStatusChanged(IBehavior arg1, BehaviorStatus from, BehaviorStatus to)
        {

            switch (behavior.Status)
            {
                case BehaviorStatus.Disposed:
                    OnDisposed();
                    break;
                case BehaviorStatus.Uninitialized:
                case BehaviorStatus.Initialized:
                case BehaviorStatus.Failed:
                case BehaviorStatus.Succeeded:                
                    StopCoroutine();
                    break;
                case BehaviorStatus.Running:
                    if (from == BehaviorStatus.Suspended)
                    {
                        Resume();
                    }
                    else
                    {
                        StartCoroutine();
                    }
                    break;
                case BehaviorStatus.Suspended:
                    Suspend();
                    break;
                default:
                    break;
            }
        }

        #endregion

        private void StartCoroutine()
        {
            coroutine = Coroutine.Start(this, behavior.Context, this.recurranceParameters);
        }


        private void Resume()
        {
            // FUTURE: allow resume of coroutine?
            if (coroutine != null)
            {
                coroutine.Resume();
            }
            else
            {
                StartCoroutine();
            }
        }

        private void Suspend()
        {
          
#if BehaviorPoller_SuspendAndResume
#endif
            if (coroutine != null)
            {
                coroutine.Suspend();
            }
            StopCoroutine();

            var c = coroutine;
            if (c != null)
            {
                coroutine = null;
                c.Dispose();
            }
        }

        private void StopCoroutine()
        {
            var c = coroutine;
            if (c != null)
            {
                coroutine = null;
                c.Dispose();
            }
        }



        #region IEnumerator Pass-through

        public object Current
        {
            get { return enumerator.Current; }
        }

        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            enumerator.Reset(); // Not supported?
        }

        #endregion

        public override string ToString()
        {
            return (Name ?? "Poller") + " for " + behavior.ToString();
        }

        public string Name;
    }
}
