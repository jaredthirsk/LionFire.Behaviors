#define LOG_STATUS
#define LOG_DEINITIALIZING_EXCEPTIONS
#define TRACE_CornerCases
//#define LOG_STATUS_DEINITIALIZE
//#define LOG_STATUS_INITIALIZE

//#define BEHAVIOR_CHANGEABLE_CONTEXT
using LionFire.Coroutines;
using LionFire.Meta;
//using LionFire.Structures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{

    /// <summary>
    /// TODO: Dispose strategy
    ///  - Keep children 
    ///  
    /// FUTURE:
    ///  - Timeout
    ///  - Retries
    ///  - History of success/failure, moving average.  Per unit and team and global
    /// </summary>
    public abstract class BehaviorBase : IBehavior
        //EnableableBase,

#if BEHAVIOR_INPC
, INotifyPropertyChanged
#endif
    {
        #region Constants

        public const string ContextMissingMessage = "Context missing";

        #endregion

        #region Ontology

        //public string Name { get; set; }
        //public object Owner { get; set; }

        #region Parent

#if !NO_BEHAVIOR_PARENTS
        [SetOnce]
        public IBehavior Parent
        {
            get { return parent; }
            set
            {
                if (parent == value) return;

                if (value == null)
                {
                    Context = null;
                }
                else if (parent != null) throw new AlreadySetException();
                                
                parent = value;

                if (Context == null && parent != null)
                {
                    this.Context = parent.Context;
                }
            }
        } private IBehavior parent;
#endif

        #endregion

        #region Configuration

        public virtual bool AllowsSuspend { get { return false; } }

        #endregion

        #region Context

        public object Context
        {
            get {
                if (context == null && Parent!=null)
                {
                    context = Parent.Context; // TODO REVIEW -- too much automatic stuff going on.  Maybe eliminate this one?  
                }
                return context; }
            set
            {
                if (context == value) return;
                if (context != null && value != null)
                {

#if BEHAVIOR_CHANGEABLE_CONTEXT
                    //l.Warn("Changing context");
#else
                    throw new InvalidOperationException("Cannot change Context once it has been set");
#endif
                }
                context = value;
            }
        } private object context;

        #endregion
                
#if BEHAVIOR_NAME
        public string Name { get; set; }
#endif
        #endregion

        #region Construction

        public BehaviorBase()
        {
#if NO_BEHAVIOR_PARENTS
            if (Context == null)
            {
                Context = BehaviorContext.Context;
            }
#endif
        }

        #endregion
        #region (Public) Status Change Methods

        public BehaviorStatus Initialize()
        {
            switch (Status)
            {
                case BehaviorStatus.Uninitialized:
                    if (OnInitializing())
                    {
                        Status = BehaviorStatus.Initialized;
                    }
                    break;
                case BehaviorStatus.Initialized:
                    break;
                case BehaviorStatus.Running:
                    throw new InvalidOperationException("Cannot be invoked while the behavior is running");
                //case BehaviorStatus.Failed:
                //    break;
                //case BehaviorStatus.Succeeded:
                //    break;
                case BehaviorStatus.Disposed:
                    break;
                default:
                    Status = BehaviorStatus.Initialized;
                    break;
            }
            return Status;
        }

        #region Start

        public BehaviorStatus Start()
        {
            switch (Status)
            {
                case BehaviorStatus.Uninitialized:
                    if (Initialize() == BehaviorStatus.Initialized)
                    {
                        goto case BehaviorStatus.Initialized;
                    }
                    // else, must have failed
                    break;
                case BehaviorStatus.Initialized:
                    {
                        OnStarting();
                        var status = OnStart();
                        Status = status;
                        return status;
                    }
                case BehaviorStatus.Suspended:
                    {
                        var status = OnResume();
                        Status = status;
                        return status;
                    }
                case BehaviorStatus.Running:
#if SanityChecks

#endif
                    break;
                case BehaviorStatus.Failed:
                case BehaviorStatus.Succeeded:
                    Deinitialize();

                    Contract.Assert(Status == BehaviorStatus.Uninitialized);

                    goto case BehaviorStatus.Uninitialized;

                //throw new InvalidOperationException("Behavior has terminated.  Invoke Initialize before Starting it again");
                case BehaviorStatus.Disposed:
                    throw new ObjectDisposedException(this.ToString());
                default:
                    throw new UnreachableCodeException();
            }

            return Status;
        }

        protected virtual void OnStarting()
        {
        }

        /// <summary>
        /// Override this to implement start behavior.
        /// Must set this.Status to BehaviorStatus.Running (or Success or Fail) 
        /// </summary>
        /// <returns>Resulting Status</returns>
        /// <remarks>
        /// Only be called from Start() method
        /// </remarks>
        protected abstract BehaviorStatus OnStart();

        //public virtual BehaviorStatus Start()
        //{
        //    this.Status = BehaviorStatus.Running;
        //    return Status; // May be Running, for long-running behaviors, or Failure or Success for a quick result. 
        //}

        //public BehaviorStatus StartAsync()
        //{
        //     Singleton<DefaultCoroutineHostProvider>.Instance.
        //    this.Status = BehaviorStatus.Running;
        //}

        #endregion

        #region Cancel

        /// <summary>
        /// Does nothing if Behavior is not Running.
        /// </summary>
        /// <param name="msg">Optional message</param>
        public void Cancel(string msg = null)
        {
            if (Status == BehaviorStatus.Running)
            {
                OnCanceling(msg);                
            }
        }

        

        #region SucceedWhenDone

        /// <summary>
        /// True by default.
        /// Used by normal behaviors when canceling.  Used by Conditions when the condition is met.
        /// </summary>
        public bool SucceedWhenDone
        {
            get { return succeedWhenDone; }
            set { succeedWhenDone = value; }
        } private bool succeedWhenDone = true;

        #endregion

        //protected virtual void OnCanceling(string msg = null) { Fail(msg); }
        protected virtual void OnCanceling(string msg = null)
        {
            if (SucceedWhenDone) Succeed();
            else Fail(msg);
        }

        #endregion

        #region Suspend / Resume

        public void Suspend()
        {
            switch (Status)
            {
                //case BehaviorStatus.Uninitialized:
                //    break;
                //case BehaviorStatus.Initialized:
                //    break;
                case BehaviorStatus.Running:
                    OnSuspending();
                    Status = BehaviorStatus.Suspended;
                    break;
                //case BehaviorStatus.Failed:
                //    break;
                //case BehaviorStatus.Succeeded:
                //    break;
                //case BehaviorStatus.Disposed:
                //    break;
                //case BehaviorStatus.Suspended:
                    //break;
                default:
                    break;
            }
        }

        public void Resume()
        {
            Start();
        }

        protected virtual void OnSuspending() { }

        /// <summary>
        /// Default implementation executes OnStart.
        /// </summary>
        protected virtual BehaviorStatus OnResume() { return OnStart(); }

        #endregion

        #region Wait

        public void WaitUntilFinished()
        {
            while (IsRunning) // TODO - Use a ResetEvent?
            {
#if NET35
                System.Threading.Thread.Sleep(0);
#else
                System.Threading.Thread.Yield();
#endif
            }
        }

        #endregion

        public void Deinitialize()
        {
            if (Status == BehaviorStatus.Running)
            {
                Cancel();
            }
            
            if (Status == BehaviorStatus.Disposed || Status == BehaviorStatus.Uninitialized) return;

            try
            {
                OnDeinitializing();
            }

#if LOGGER && LOG_DEINITIALIZING_EXCEPTIONS
            catch(Exception ex)
            {
                l.Error(ex.ToString());
#else
            catch { 
#endif
            }
            Status = BehaviorStatus.Uninitialized;
        }

        protected virtual void OnDeinitializing()
        {
        }

        #endregion
		
        #region Status Transitions

        /// <summary>
        /// Throw an exception (slow) or return false for failure
        /// </summary>
        protected virtual bool OnInitializing()
        {
            return true;
        }

        protected void Succeed()
        {
#if SanityChecks
            if (Status == BehaviorStatus.Succeeded)
            {
                l.Trace("Succeed() - Already succeeded: " + this.ToString());
            }
#endif
            switch (Status)
            {
                case BehaviorStatus.Disposed:
                    l.Trace(this.ToString() + " Succeed() called when Disposed");
                    break;
                //case BehaviorStatus.Uninitialized: // Succeeded at initialize time?
                case BehaviorStatus.Initialized: // Ran quickly and succeeded
                case BehaviorStatus.Running:
                case BehaviorStatus.Succeeded:
                    Status = BehaviorStatus.Succeeded;
                    break;
                default:
                    throw new InvalidOperationException("Invalid state for Succeed()");
            }
        }

        protected void FaultOnMissing(Type type)
        {
            Fault("Missing dependency of type " + type.Name);
        }
        protected void FaultOnMissing<type>()
        {
            Fault("Missing dependency of type " + typeof(type).Name);
        }
        protected void FailOnMissing<type>()
        {
            Fail("Requires a " + typeof(type).Name);
        }

        protected void Fault(string msg = null)
        {
            l.Error("FAULT: " + msg + " " + Environment.StackTrace);
            // FUTURE: maybe have a fault status code?
            Fail(msg);
        }

        protected void Fail(string msg = null)
        {
            switch (Status)
            {
                case BehaviorStatus.Uninitialized: // Failed to initialize
                case BehaviorStatus.Initialized: // Failed to start, or ran quickly
                case BehaviorStatus.Running:
                    {
                        this.StatusMessage = msg; // Do this before setting Status, so StatusChanged handler can process it.
                        Status = BehaviorStatus.Failed;
                        break;
                    }
                case BehaviorStatus.Succeeded:
#if TRACE_CornerCases
                    l.Trace("Already succeeded.  Ignoring Fail.");
#endif
                    break;
                case BehaviorStatus.Failed:
                    l.Warn("Already failed. " + this.ToString());
                    break;
                default:
                    throw new InvalidOperationException("Invalid state for Fail()");
            }
        }

        #endregion

        #region Status

        public bool IsDisposed { get { return Status == BehaviorStatus.Disposed; } }
        public bool IsRunning { get { return Status == BehaviorStatus.Running; } set { if (value) Start(); else Cancel(); } }
        public bool IsFinished { get { return Status == BehaviorStatus.Failed || Status == BehaviorStatus.Succeeded; } }

        public static readonly bool DeinitializeOnFinish = false;

        protected virtual void OnFinished()
        {
            if (DeinitializeOnFinish) Deinitialize();
        }

        public BehaviorStatus Status
        {
            get { return status; }
            private set
            {
                lock (statusLock)
                {
                    if (status == value) return;
                    var oldStatus = status;
                    if (oldStatus == BehaviorStatus.Disposed)
                    {
                        throw new ObjectDisposedException(this.ToString());
                    }

                    status = value;


#if LOG_STATUS
                    var msg = "[" 
                        //+ oldStatus.ToString().ToUpper() + " --> " 
                         + StatusWord.ToString().ToUpper()
                        //+ status.ToString().ToUpper()
                        .Substring(0,1)
                        + "] " + this.ToString();
#endif
                    switch (status)
                    {
                        case BehaviorStatus.Uninitialized:
#if LOG_STATUS_DEINITIALIZE
                            l.Trace(msg);
#endif
                            break;
                        case BehaviorStatus.Initialized:
#if LOG_STATUS_INITIALIZE
                            l.Trace(msg);
#endif
                            break;
                        case BehaviorStatus.Running:
#if LOG_STATUS
                            l.Debug(msg);
#endif
                            break;
                        case BehaviorStatus.Failed:
#if LOG_STATUS
                            l.Info(msg);
#endif

                            OnFinished();
                            break;
                        case BehaviorStatus.Succeeded:
#if LOG_STATUS
                            l.Debug(msg);
#endif
                            OnFinished();
                            break;
                        case BehaviorStatus.Disposed:
#if LOG_STATUS
                            l.Debug(msg);
#endif
                            break;
                        default:
                            break;
                    }

                    OnStatusChangedFrom(oldStatus);

                    RaiseStatusChanged(oldStatus);

#if BEHAVIOR_INPC
                    OnPropertyChanged("Status");
                    OnPropertyChanged("StatusText");
#endif
                }
            }
        } private BehaviorStatus status;
        protected object statusLock = new object();

        protected abstract void RaiseStatusChanged(BehaviorStatus oldStatus);
        public abstract event Action<IBehavior, BehaviorStatus, BehaviorStatus> StatusChangedForFromTo;

        protected virtual void OnStatusChangedFrom(BehaviorStatus oldStatus)
        {
            switch (Status)
            {
                case BehaviorStatus.Uninitialized:
                    OnDeinitializing();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region IsMonitoring

        /// <summary>
        /// 
        /// For a IScorer
        ///  - IRUSF Keep score up to date
        /// 
        /// For a ICondition
        ///  - Active (Succeeded) true, but keep checking 
        ///  - Watching (Fail) false, but keep checking
        /// 
        /// For a Behavior
        ///  - Waiting (suspended) - will wake itself
        /// 
        /// Can only be true if Initialized / Suspended / Running 
        /// or Succeed/Fail if ICondition
        /// </summary>
        public bool IsMonitoring
        {
            get { return isMonitoring; }
            set
            {
                if (isMonitoring == value) return;
                isMonitoring = value;
                OnMonitoringChanged();
            }
        } private bool isMonitoring;

        protected virtual void OnMonitoringChanged()
        {
        }

        #endregion

        protected virtual void OnRunning()
        {
        }

        #region StatusMessage

        public string StatusMessage
        {
            get { return statusMessage; }
            set
            {
                if (statusMessage == value) return;
                statusMessage = value;

                //var ev = StatusMessageChangedForTo;
                //if (ev != null) ev(this, value);
            }
        } private string statusMessage;

        //public event Action<BehaviorNode, string> StatusMessageChangedForTo;

        #endregion


        #region StatusText

        public virtual string StatusText
        {
            get { return this.ToString(); }
            //            set
            //            {
            //                if (statusText == value) return;
            //                statusText = value;
            //#if BEHAVIOR_INPC
            //                OnPropertyChanged("StatusText");
            //#endif
            //            }
        } //private string statusText;

        protected void RaiseStatusTextChanged()
        {
#if BEHAVIOR_INPC
            OnPropertyChanged("StatusText");
#endif
        }
        #endregion

#if BEHAVIOR_DISPOSE
        public virtual void Dispose()
        {
#if !NO_BEHAVIOR_PARENTS
            Parent = null;
#endif
            Cancel("Disposing");
            status = BehaviorStatus.Disposed;
        }
        
#endif

        #region Misc

        #region INotifyPropertyChanged Implementation

#if BEHAVIOR_INPC
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var ev = PropertyChanged;
            if (ev != null) ev(this, new PropertyChangedEventArgs(propertyName));
        }
#else
        [Conditional("BEHAVIOR_INPC")]
        protected void OnPropertyChanged(string propertyName){}
#endif

        #endregion
        
//#if LOGGER
        private static ILogger l = Log.Get();
//#endif

        public virtual string StatusWord
        {
            get
            {
                switch (Status)
                {
                    case BehaviorStatus.Uninitialized:
                        break;
                    case BehaviorStatus.Initialized:
                        if (IsMonitoring) return "Wait";
                        break;
                    case BehaviorStatus.Running:
                        if (IsMonitoring) return "Active";
                        break;
                    case BehaviorStatus.Failed:
                        break;
                    case BehaviorStatus.Succeeded:
                        if (IsMonitoring) return "Monitoring";
                        break;
                    case BehaviorStatus.Disposed:
                        break;
                    case BehaviorStatus.Suspended:
                        if (IsMonitoring) return "Waiting";
                        break;
                    default:
                        break;
                }
                return Status.ToString();
            }
        }
        public override string ToString()
        {
            string msg = "[" + this.GetType().Name
#if BEHAVIOR_NAME
 + (String.IsNullOrEmpty(this.Name) ? "" : " \"" + this.Name + "\"")
#endif
 + " " + Status.ToString().ToUpper() + "]";
            return msg;
        }
        #endregion
    }

}
