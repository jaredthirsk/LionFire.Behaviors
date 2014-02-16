using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    /// <summary>
    /// Run all children in parallel, wait for one to succeed or all to fail.  Return success if at least one child succeeded.
    /// </summary>
    public sealed class TryOne : Parallel
    {
        public override bool StopOnOneSuccess { get { return true; } }
        public override bool StopOnOneFail { get { return false; } }
        public override bool SucceedOnOneSucceed { get { return true; } }
    }

    /// <summary>
    /// Run all children in parallel, wait for all to finish.  Return success if at least one child succeeded.
    /// </summary>
    /// <remarks>
    /// FUTURE Idea: Max parallelism to limit running children to a certain amount
    /// </remarks>
    public sealed class TryAll : Parallel
    {
        //public virtual bool StopOnOneSuccess { get { return false; } }
        public override bool StopOnOneFail { get { return false; } }
        public override bool SucceedOnOneSucceed { get { return true; } }
    }
    
    /// <summary>
    /// Returns Succeeded when one children succeeds.  Returns Fail as soon as one child fails.
    /// </summary>
    /// <remarks>
    /// Can be used for a while(conditions) { do something } loop: 
    ///  - The conditions stay at Running until stopped or until their condition returns Fail.
    ///  - The do something child executes a task and returns success.
    /// </remarks>
    public sealed class ParallelOne : Parallel
    {
        public override bool StopOnOneSuccess { get { return true; } }
        //public virtual bool StopOnOneFail { get { return true; } }
        public override bool SucceedOnOneSucceed { get { return true; } }
    }

    /// <summary>
    /// Returns success or fail of first item to complete.
    /// </summary>
    public sealed class ParallelFirst : Parallel
    {
        public override bool StopOnOneSuccess { get { return true; } }
        public override bool StopOnOneFail { get { return true; } }
        //public virtual bool SucceedOnOneSucceed { get { return false; } }
    }

    /// <summary>
    /// Returns Succeeded when all children Succeed.  Returns Fail as soon as one child fails.
    /// </summary>
    public class Parallel : MultiComposite
    {
        #region Parameters

        // TOOPTIMIZE - flatten class hierarchy, remove virtuals?

        public virtual bool StopOnOneSuccess { get { return false; } }

        /// <summary>
        /// If true, stop all children when one child fails.  If false, keep running even if one child fails.
        /// </summary>
        public virtual bool StopOnOneFail { get { return true; } }

        /// <summary>
        /// If true, this node returns success if any child returned success.  If false,
        /// all children must succeed in order for this to be a success.
        /// </summary>
        public virtual bool SucceedOnOneSucceed { get { return false; } }

        #endregion

        #region Monitor Statuses

        protected override void OnChildStatusChanged(IBehavior child, BehaviorStatus oldStatus, BehaviorStatus childStatus)
        {
            switch (childStatus)
            {
                case BehaviorStatus.Initialized:
                    break;
                case BehaviorStatus.Running:
                    break;
                case BehaviorStatus.Failed:
                    OnChildFailed(child);
                    break;
                case BehaviorStatus.Succeeded:
                    OnChildSucceeded(child);
                    break;
                case BehaviorStatus.Disposed:
                    break;
                default:
                    break;
            }
            base.OnChildStatusChanged(child, oldStatus, childStatus);
        }

#endregion

        #region Status event handlers

        private void OnChildFailed(IBehavior child)
        {
            if (StopOnOneFail || AllChildrenFailed)
            {
                MonitoringChildren = false;
                CancelChildren();
                if (SucceedOnOneSucceed)
                {
#if AOT
                    throw new NotSupportedException("TOAOT")
#endif
                    children.Where(c => c.Status == BehaviorStatus.Succeeded).Any();
                }
                else
                {
                    Fail();
                }
            }
        }

        private void OnChildSucceeded(IBehavior child)
        {
            bool allSucceeded = AllChildrenSucceeded;
            if (StopOnOneSuccess || allSucceeded)
            {
                MonitoringChildren = false;
                if (SucceedOnOneSucceed)
                {
                    //foreach (var childX in Children)
                    //{
                    //    childX
                    //}
                }
                Succeed();
            }
        }

        #endregion

        #region Child Queries

        protected bool AllChildrenSucceeded
        {
            get
            {
#if AOT
                    throw new NotSupportedException("TOAOT")
#endif
                foreach (var child in children)
                {
                    if (child.Status != BehaviorStatus.Succeeded)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        protected bool AllChildrenFailed
        {
            get
            {
#if AOT
                    throw new NotSupportedException("TOAOT")
#endif
                foreach (var child in children)
                {
                    if (child.Status != BehaviorStatus.Failed)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        #endregion

        #region Transitions

        protected override BehaviorStatus OnStart()
        {
            MonitoringChildren = true; // Handles children failing/succeeding

            lock (statusLock)
            {
                foreach (var child in children)
                {
                    var childResult = child.Start();

                    l.Trace("Parallel start child: " + child.ToString());

                    if (IsFinished) return this.Status;
                }
                if (children.Count == 0)
                {
                    return BehaviorStatus.Succeeded; // Fail if no children?
                }
                else
                {
                    return BehaviorStatus.Running; // There must be at least one child running, or else IsFinished would have been set above.  THREADSAFETY - REVIEW?
                }
            }
        }

        #endregion

        private static ILogger l = Log.Get();
    }
}
