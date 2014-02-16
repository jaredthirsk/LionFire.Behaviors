#if NoDeps
#define CoroutineNames
//#define OwnerCanBeCoroutineHost
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Coroutines
{
    public class Coroutine : IDisposable
    {
        #region Static

        public static CoroutineHost DefaultCoroutineHost;

        #endregion

        //private CoroutineHost host { get { return State.Host} }
        public IEnumerator Enumerator { get { return State.Enumerator; } }

        #region State

        internal CoroutineState State
        {
            get { if (state == null) throw new ObjectDisposedException(String.Empty); return state; }
            private set { state = value; }
        } private CoroutineState state;

        #endregion

        public void Suspend()
        {
        }

        public void Resume()
        {
        }

        #region RecurranceParameters

        /// <summary>
        /// Can be changed while coroutine is running.  Changes will not take effect
        /// until after the next time the coroutine is executed.
        /// </summary>
        public RecurranceParameters RecurranceParameters
        {
            get { return State.RecurranceParameters; }
            private set { State.RecurranceParameters = value; }
        }

        #endregion

        #region Construction

        private Coroutine() { }
        //private Coroutine(RecurranceParameters recurranceParameters = null)
        //{
        //    this.RecurranceParameters = recurranceParameters;
        //}

        public static Coroutine Start(IEnumerator coroutine, object owner = null, RecurranceParameters recurranceParameters = null)
        {
            var host = GetCoroutineHost(owner);

            var c = new Coroutine();
            c.State = host.EnqueueCoroutine(coroutine, recurranceParameters);
            return c;
        }

        /// <summary>
        /// Run coroutine once immediately.  Returns null if the coroutine finished.
        /// </summary>
        /// <param name="owner">owner must provide a CoroutineHost by implementing IHasCoroutineHost.CoroutineHost, or the static Coroutine.DefaultCoroutineHost must be set</param>
        /// <param name="coroutine"></param>
        /// <param name="recurranceParameters"></param>
        /// <returns></returns>
        public static Coroutine StartImmediately(IEnumerator coroutine, object owner = null, RecurranceParameters recurranceParameters = null)
        {
            var host = GetCoroutineHost(owner);

            var state = host.RunCoroutine(coroutine, recurranceParameters);
            if (state == null) return null; // Finished right away

            var c = new Coroutine();
            c.State = state;
            return c;
        }

        private static CoroutineHost GetCoroutineHost(object owner)
        {
            CoroutineHost host=null;

#if OwnerCanBeCoroutineHost
            host = owner as CoroutineHost;
            if (host != null) return host;
#endif

            var hasCH = owner as IHasCoroutineHost;
            if(hasCH!=null) host = hasCH.CoroutineHost;
            if (host != null) return host;

            host = DefaultCoroutineHost;
            if (host != null) return host;

#if OwnerCanBeCoroutineHost
            throw new ArgumentException("owner must be a CoroutineHost or provide one by implementing IHasCoroutineHost.CoroutineHost, or the static Coroutine.DefaultCoroutineHost must be set");
#else
            throw new ArgumentException("owner must provide a CoroutineHost by implementing IHasCoroutineHost.CoroutineHost, or the static Coroutine.DefaultCoroutineHost must be set");
#endif
        }

        #endregion

        #region Destruction

        public bool IsDisposed { get { return state == null; } }
        public void Dispose()
        {
            var s = State;
            State = null;
            if (s != null)
            {
                s.Dispose();
            }
        }

        #endregion



        [Conditional("CoroutineNames")]
        public void SetName(string name)
        {
            State.SetName(name);
        }
    }
}
#endif