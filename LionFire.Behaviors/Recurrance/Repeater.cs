using LionFire.Coroutines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public class IntervalRepeater : Repeater
    {
        #region Parameters

        public RecurranceParameters RecurranceParameters { get; set; }

        #endregion

        #region State

        private object locker = new object();
        int backlog = 0; // TODO: Attach to child status and when it finishes, decrement backlog and launch another

        RecurranceParameters coroutineRecurranceParameters;
        BehaviorCoroutine behaviorCoroutine;

        #endregion

        #region Construction

        public IntervalRepeater(IBehavior behavior, RecurranceParameters recurranceParameters = null)
            : base(behavior)
        {
            this.RecurranceParameters = recurranceParameters;
        }

        #endregion

        protected override BehaviorStatus OnStart()
        {
            coroutineRecurranceParameters = new RecurranceParameters() // TODO - clone
            {
                Interval = this.RecurranceParameters.Interval,
                StartImmediately = this.RecurranceParameters.StartImmediately,
            };
            
            if (behaviorCoroutine == null)
            {
                behaviorCoroutine = new BehaviorCoroutine(this, CheckTime, coroutineRecurranceParameters);
            }
            // else bc will respond to start

            //Coroutine.Start(CheckTime, this.Context, this.RecurranceParameters);
            return base.OnStart();
        }

        protected IEnumerator CheckTime
        {
            // TODO: Verify the time actually elapsed, or not too much or too little.
            // Adjust coroutineRecurranceParameters.Interval for remaining time, like with Delay
            get
            {
                while (true)
                {
                    try
                    {
                        lock (locker)
                        {
                            switch (Child.Status)
                            {
                                default:
                                    //case BehaviorStatus.Uninitialized:
                                    //case BehaviorStatus.Initialized:
                                    //Child.StatusChangedForFromTo += OnChildStatusChangedForFromTo;
                                    //Child.Start();
                                    TryStartAgain();
                                    break;
                                case BehaviorStatus.Running:
                                    // Already started!
                                    if (UseBacklog)
                                    {
                                        backlog++;
                                    }
                                    break;
                                //case BehaviorStatus.Failed:
                                //    break;
                                //case BehaviorStatus.Succeeded:
                                //    break;
                                //case BehaviorStatus.Disposed:
                                //    break;
                            }

                            //if (behavior.Start != BehaviorStart.Running)
                            //{
                            //    IsEnabled = false;
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        l.Error(ex.ToString());
                    }

                    if (IsRunning)
                    {
                        yield return null;
                    }
                    else
                    {
                        l.Trace("Repeater not running anymore. Leaving CheckTime loop.");
                        yield break;
                    }
                }
            }

        }

        protected override void OnAfterChildFinished()
        {
            // Disable the base class behavior
        }

        #region Misc

        private static ILogger l = Log.Get();
		
        #endregion
    }

    public class Repeater : Decorator
    {
        #region Parameters

        /// <summary>
        /// If greater than 0, stop this poller after task fails.
        /// TODO
        /// </summary>
        public int StopAfterFailures = 0;
        
        public int StopAfterSuccesses = 0;

        public int MaxIterations = 0;

        /// <summary>
        /// TODO - If true, don't start the timer for the next task until the current one finishes.
        /// </summary>
        public bool IntervalStartsOnTaskEnd;

        /// <summary>
        /// Only used if IntervalStartsOnTaskEnd is false.  If the timer goes off repeatedly before the task is done,
        /// ensure the tasks are still executed multiple times.
        /// </summary>
        public bool UseBacklog;

        public bool ReuseChild = true;

        #endregion

        #region State

        protected int StopAfterFailuresRemaining = 0;
        protected int StopAfterSuccessesRemaining = 0;
        protected int IterationsRemaining = 0;

        #endregion

        #region Construction

        public Repeater() { }
        public Repeater(IBehavior childBehavior)
            : base(childBehavior)
        {
        }

        #endregion

        #region Transitions

        protected override bool OnInitializing()
        {
            StopAfterFailuresRemaining = StopAfterFailures;
            StopAfterSuccessesRemaining = StopAfterSuccesses;

            return base.OnInitializing();
        }
        protected override BehaviorStatus OnStart()
        {
            MonitoringChildren = true;
            return base.OnStart();
        }

        #endregion

        #region Child Event Handling
        
        protected override void OnChildStatusChanged(IBehavior child, BehaviorStatus from, BehaviorStatus newStatus)
        {
            switch (newStatus)
            {
                case BehaviorStatus.Uninitialized:
                    break;
                case BehaviorStatus.Initialized:
                    break;
                case BehaviorStatus.Running:
                    break;
                case BehaviorStatus.Failed:
                    OnChildFinished(false);
                    break;
                case BehaviorStatus.Succeeded:
                    OnChildFinished(true);
                    break;
                case BehaviorStatus.Disposed:
                    break;
                default:
                    break;
            }
        }

        protected void OnChildFinished(bool success)
        {
            if (MaxIterations > 0)
            {
                IterationsRemaining--;
                l.Info(IterationsRemaining + " iterations remaining");
            }
            if (success)
            {
                if (StopAfterSuccessesRemaining > 0)
                {
                    StopAfterSuccessesRemaining--;
                    l.Info(StopAfterSuccessesRemaining + " successes remaining");
                    if (StopAfterSuccessesRemaining <= 0)
                    {
                        Succeed();
                        return;
                    }
                }
            }
            else
            {
                if (StopAfterFailuresRemaining > 0)
                {
                    StopAfterFailuresRemaining--;
                    l.Info(StopAfterFailuresRemaining + " failures remaining");
                    if (StopAfterFailuresRemaining <= 0)
                    {
                        Fail("Too many failures");
                        return;
                    }
                }
            }
            if (MaxIterations > 0)
            {
                if (IterationsRemaining <= 0)
                {
                    Succeed(); // "Finished all iterations"
                    return;
                }
            }
            if (base.RemoveChildrenOnFinish)
            {
                Child = null;
            }
            OnAfterChildFinished();
        }

        /// <summary>
        /// This gets disabled in the derived IntervalRepeater class
        /// </summary>
        protected virtual void OnAfterChildFinished()
        {
            TryStartAgain(); 
        }

        #endregion

        protected void TryStartAgain()
        {
            if (!IsRunning) return;

            if (!ReuseChild)
            {
                if (ChildFactory == null)
                {
                    throw new ArgumentNullException("ChildFactory must be set when ReuseChild == false");
                }
                Child = ChildFactory();
                l.Info("UNTESTED - Recreate child");
            }
            if (Child == null)
            {
                throw new ArgumentNullException("Child is null when trying to start it again.  If you set RemoveChildrenOnFinish to true, you must also set a ChildFactory.");
            }
            Child.Start();
        }

        #region Misc

        private static ILogger l = Log.Get();
		
        #endregion
    }

    // TODO - Repeater shouldn't be a PollerBase, it should be a Decorator with a poller.  That way,
    // its child.Parent will get set to self.
    public class OldPollingRepeater : PollerBase // TODO - unfinished
    {
        // Change to  Decorator.Child?
        IBehavior repeaterChild { get { return PollTarget as IBehavior; } }
        object locker = new object();

        #region Parameters

        /// <summary>
        /// If greater than 0, stop this poller after task fails.
        /// TODO
        /// </summary>
        public int StopAfterFailures = 0;
        private int StopAfterFailuresRemaining = 0;
        public int StopAfterSuccesses = 0;
        private int StopAfterSuccessesRemaining = 0;

        /// <summary>
        /// TODO - If true, don't start the timer for the next task until the current one finishes.
        /// </summary>
        public bool IntervalStartsOnTaskEnd;

        /// <summary>
        /// Only used if IntervalStartsOnTaskEnd is false.  If the timer goes off repeatedly before the task is done,
        /// ensure the tasks are still executed multiple times.
        /// </summary>
        public bool UseBacklog;

        #endregion

        #region State

        int backlog = 0; // TODO: Attach to child status and when it finishes, decrement backlog and launch another

        #endregion

        #region Construction

        public OldPollingRepeater(IBehavior behavior, RecurranceParameters recurranceParameters = null)
        {
            l.Warn("TODO: Obsolete OldPollingRepeater - get interval support in new Repeater");
            behavior.Parent = this; // TODO - how to automatically set Parents 
            //this.behavior = behavior;

            //if (recurranceParameters == null)
            //{
            //    var hasRP = behavior as IHasStartRecurranceParameters;
            //    if (hasRP != null)
            //    {
            //        recurranceParameters = hasRP.StartRecurranceParameters;
            //    }
            //}
            SetPolledAndRP(behavior, recurranceParameters);
        }

        #endregion

        protected override void OnStarting()
        {
            if (IsRunning)
            {
                // TODO Move to Initialize
                StopAfterFailuresRemaining = StopAfterFailures;
                StopAfterSuccessesRemaining = StopAfterSuccesses;
            }
        }

        protected override void DoPoll()
        {
            try
            {
                lock (locker)
                {
                    switch (repeaterChild.Status)
                    {
                        default:
                            //case BehaviorStatus.Uninitialized:
                            //case BehaviorStatus.Initialized:
                            repeaterChild.StatusChangedForFromTo += OnChildStatusChangedForFromTo;
                            repeaterChild.Start();
                            break;
                        case BehaviorStatus.Running:
                            // Already started!
                            if (UseBacklog)
                            {
                                backlog++;
                            }
                            break;
                        //case BehaviorStatus.Failed:
                        //    break;
                        //case BehaviorStatus.Succeeded:
                        //    break;
                        //case BehaviorStatus.Disposed:
                        //    break;
                    }


                    //if (behavior.Start != BehaviorStart.Running)
                    //{
                    //    IsEnabled = false;
                    //}
                }
            }
            catch (Exception ex)
            {
                l.Error("ScorePoller - UpdateScore threw exception: " + ex.ToString());
            }
        }

        void OnChildStatusChangedForFromTo(IBehavior child,  BehaviorStatus from, BehaviorStatus newStatus)
        {
            switch (newStatus)
            {
                case BehaviorStatus.Uninitialized:
                    break;
                case BehaviorStatus.Initialized:
                    break;
                case BehaviorStatus.Running:
                    break;
                case BehaviorStatus.Failed:
                    OnChildFinished();
                    if (StopAfterFailuresRemaining > 0)
                    {
                        StopAfterFailuresRemaining--;
                        if (StopAfterFailuresRemaining <= 0)
                        {
                            this.IsRunning = false;
                            l.Error("Too many failures.  ");
                            // TODO: Make this Poller an activity?  One that is isolated in its own tree? (Except for in TaskRunner)
                        }
                    }
                    break;
                case BehaviorStatus.Succeeded:
                    OnChildFinished();
                    if (StopAfterSuccessesRemaining > 0)
                    {
                        StopAfterSuccessesRemaining--;
                        if (StopAfterSuccessesRemaining <= 0)
                        {
                            Succeed();
                        }
                    }
                    break;
                case BehaviorStatus.Disposed:
                    break;
                default:
                    break;
            }
        }
        private void OnChildFinished()
        {
            this.repeaterChild.StatusChangedForFromTo -= OnChildStatusChangedForFromTo;
        }

        #region Misc

        private static ILogger l = Log.Get();

        #endregion
    }

}
