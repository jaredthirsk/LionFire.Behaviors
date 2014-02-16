#if  NoDeps
#define CoroutineNames
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Coroutines
{
    /// <summary>
    /// Held by Coroutine
    /// </summary>
    internal class CoroutineState : IDisposable
    {
        #region Parameters

        public IEnumerator Enumerator { get; private set; }

        #region RecurranceParameters

        /// <summary>
        /// Can be changed while coroutine is running.  Changes will not take effect
        /// until after the next time the coroutine is executed.
        /// </summary>
        public RecurranceParameters RecurranceParameters
        {
            get { return recurranceParameters; }
            set
            {
                recurranceParameters = value;
                OnRecurranceParametersIntervalEnabledChanged();
            }
        } private RecurranceParameters recurranceParameters;

        public void OnRecurranceParametersIntervalEnabledChanged()
        {
            if (RecurranceParameters != null)
            {
                ResetWaitTimeTicks = DefaultResetWaitTimeTicks;
                if (!RecurranceParameters.StartImmediately)
                {
                    WaitTimeTicks = RecurranceParameters.StopwatchInterval;
                    return;
                }
                else
                {
                    WaitTimeTicks = 0;
                }
            }
            else
            {
                ResetWaitTimeTicks = null;
                WaitTimeTicks = 0;
            }
        }

        #endregion

        #endregion

        #region Construction and Destruction

        public CoroutineState(IEnumerator enumerator, RecurranceParameters rp = null)
        {
            if (enumerator == null) throw new ArgumentNullException("enumerator");
            this.Enumerator = enumerator;

            RecurranceParameters = rp;
        }

        public void Dispose()
        {
            this.Enumerator = null;
        }

        #endregion

        public bool IsDisposed { get { return Enumerator != null; } }

        #region State

        /// <summary>
        /// Stopwatch Ticks remaining before next time the coroutine should be run
        /// </summary>
        public long WaitTimeTicks; // From Stopwatch.GetTimestamp()

        #endregion

        #region (Private) Strategies

        public Action<CoroutineState> ResetWaitTimeTicks;

        #endregion

        #region (Static)

        private static void DefaultResetWaitTimeTicks(CoroutineState s)
        {
            s.WaitTimeTicks = s.RecurranceParameters.StopwatchInterval;
        }

        #endregion

#if CoroutineNames
        public string Name { get; private set; }

        public override string ToString()
        {
            //return "(Coroutine " + (Name ?? Enumerator.GetType().FullName) + ")";
            return Enumerator.ToString();
        }
#endif

        [Conditional("CoroutineNames")]
        internal void SetName(string name)
        {
            this.Name = name;
        }
    }
}
#endif