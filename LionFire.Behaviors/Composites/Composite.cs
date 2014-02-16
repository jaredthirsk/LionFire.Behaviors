using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public interface IComposite : IBehavior
    {
        IEnumerable<IBehavior> Children { get; }
    }

    /// <summary>
    /// Base class for all behaviors that have one or more children: Decorator (one child), Selector, Sequencer, Parallel
    /// </summary>
    public abstract class Composite : Behavior, IComposite
    {
        /// <summary>
        /// True to reduce memory usage. False to reduce Garbage Collection and memory allocation.
        /// </summary>
        protected virtual bool RemoveChildrenOnFinish { get { return false; } }
        //protected virtual bool MonitorsChildStatuses { get { return false; } }
        protected virtual bool MonitorsChildScores { get { return false; } }

        #region MonitoringChildren

        public bool MonitoringChildren
        {
            get { return monitoringChildren; }
            set
            {
                if (monitoringChildren == value) return;

                monitoringChildren = value;

                foreach (var child in Children)
                {
                    //if (MonitorsChildStatuses)
                    {
                        if (value)
                        {
                            child.StatusChangedForFromTo += OnChildStatusChanged;
                        }
                        else
                        {
                            child.StatusChangedForFromTo -= OnChildStatusChanged;
                        }
                    }
                    //child.Dispose();
                }
            }
        } private bool monitoringChildren;

        protected virtual void OnChildStatusChanged(IBehavior child,  BehaviorStatus from, BehaviorStatus newStatus)
        {
        }

        #endregion

        protected override void OnStatusChangedFrom(BehaviorStatus oldStatus)
        {
            base.OnStatusChangedFrom(oldStatus);

            switch (Status)
            {
                case BehaviorStatus.Uninitialized: // Handled in base
                    break;
                case BehaviorStatus.Initialized:
                    //CreateChildrenIfNeeded();
                    foreach (var child in Children)
                    {
                        child.Initialize(); // REVIEW - do this in OnInitializing instead? (allow it to be overridden)
                    }
                    //OnInitializedChildren();
                    break;
                case BehaviorStatus.Running:
                    break;
                case BehaviorStatus.Failed:
                    break;
                case BehaviorStatus.Succeeded:
                    break;
                default:
                    throw new UnreachableCodeException();
            }
        }

        protected override bool OnInitializing()
        {
            CreateChildrenIfNeeded();
            return base.OnInitializing(); // Does nothing
        }

        protected override void OnFinished()
        {
            MonitoringChildren = false;
            if (RemoveChildrenOnFinish)
            {
                ClearChildren(); // Cancels children if running
            }
            else
            {
                CancelChildren();
            }
            base.OnFinished();
        }

        protected override void OnDeinitializing()
        {
            foreach (var child in Children)
            {
                child.Deinitialize(); // Cancels children if running
            }
            base.OnDeinitializing();
        }

        protected abstract void ClearChildren();
        protected abstract void CancelChildren();
        public abstract IEnumerable<IBehavior> Children { get; }

        protected abstract void CreateChildrenIfNeeded();

        protected virtual void OnChildAdded(IBehavior child)
        {
#if !NO_BEHAVIOR_PARENTS
            child.Parent = this;
#endif
            if (child.Context == null)
            {
                child.Context = this.Context;
            }
            if (MonitoringChildren)
            {
                child.StatusChangedForFromTo += OnChildStatusChanged;
            }
        }
        protected virtual void OnChildRemoved(IBehavior child)// TODO - invoke this in MultiComposite?
        {

            if (MonitoringChildren)
            {
                child.StatusChangedForFromTo -= OnChildStatusChanged;
            }
#if BEHAVIOR_DISPOSE
            child.Dispose();
#else
#if !NO_BEHAVIOR_PARENTS
            child.Parent = null;
#endif
            if (child.IsRunning)
            {
                child.Cancel();
            }
#endif
        }

        protected IEnumerable<IBehavior> GetNewChildrenWithContext
        {
            get
            {
#if NO_BEHAVIOR_PARENTS
                BehaviorContext.Context = this.Context; // EXPERIMENTAL OPTIMIZE
#endif
                try
                {
                    var newChildren = NewChildren
#if NO_BEHAVIOR_PARENTS
                        .ToArray(); // Force enumeration to complete while context is set.
#endif
;

#if !NO_BEHAVIOR_PARENTS
                    foreach (var nc in newChildren)
                    {
                        nc.Parent = this;
                    }
#endif
                    return newChildren;
                }

                finally
                {
#if NO_BEHAVIOR_PARENTS
                    //Thread.MemoryBarrier();
                    BehaviorContext.Context = null;
#endif
                }

            }
        }

        protected IBehavior CreateChildWithContext(Type type)
        {
#if NO_BEHAVIOR_PARENTS
            BehaviorContext.Context = this.Context;
#endif
            try
            {
                return (IBehavior)Activator.CreateInstance(type);
            }
            finally
            {
#if NO_BEHAVIOR_PARENTS
                BehaviorContext.Context = null;
#endif
            }
        }

        protected abstract IEnumerable<IBehavior> NewChildren { get; }

        //        private void OnInitializedChildren()
        //        {
        //#if !NO_BEHAVIOR_PARENTS
        //            foreach (var child in Children)
        //            {
        //                //if (MonitorsChildStatuses)
        //                //{
        //                //    child.StatusChangedForTo += OnChildStatusChanged;
        //                //}

        //                child.Parent = this;
        //            }
        //#endif
        //        }        

        //public override void Dispose()
        //{
        //    if (IsRunning)
        //    {
        //        Status = BehaviorStatus.Failed;
        //    }

        //    base.Dispose();
        //}
    }


}
