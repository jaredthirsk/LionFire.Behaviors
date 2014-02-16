using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    /// <summary>
    /// Used by Decorator, Scorer
    /// </summary>
    public abstract class SingleComposite : Composite
    {
        #region Fields

        protected Func<IBehavior> ChildFactory;

        #region Child

        public IBehavior Child
        {
            get
            {
                if (_child == null && ChildFactory != null) { Child = ChildFactory(); }
                return _child;
            }
            protected set
            {
                if (_child == value) return;

                if (_child != null)
                {
                    OnChildRemoved(_child);
                }
                _child = value;
                if (_child != null)
                {
                    OnChildAdded(_child);
                }
            }
        }
        private IBehavior _child;
        
        #endregion

        #endregion

        protected virtual Type ChildType { get { return null; } }

        #region Construction

        public SingleComposite() { }

        public SingleComposite(IBehavior behavior)
        {
            this.Child = behavior;
        }

        public SingleComposite(Func<IBehavior> childFactory)
        {
            this.ChildFactory = childFactory;
        }

        #endregion

        #region Children
        
        public sealed override IEnumerable<IBehavior> Children { get { if (Child != null)yield return Child; } }

        protected sealed override void ClearChildren()
        {
#if BEHAVIOR_DISPOSE
            if (Child != null)
            {
                Child.Dispose();
            }
#endif
            Child = null;
        }

        protected sealed override void CancelChildren()
        {
            var childCopy = _child;
            if(childCopy!=null)
            {
                childCopy.Cancel();
            }
        }

        protected sealed override void CreateChildrenIfNeeded()
        {
#if AOT
            throw new NotSupportedException();
#endif
            if (Child == null)
            {
                Child = GetNewChildrenWithContext.FirstOrDefault(); // LINQ
//#if SanityChecks
//                if (Child == null)
//                {
//                    throw new Exception("No children available");
//                }
//                //else
//                //{
//                //    System.Diagnostics.Debugger.Break();
//                //}
//#endif
            }
        }

        protected override IEnumerable<IBehavior> NewChildren
        {
            get {  if(ChildType != null) yield return CreateChildWithContext(ChildType); }
        }
        #endregion

        
        //protected override void OnStatusChangedFrom(BehaviorStatus oldStatus)
        //{
        //    base.OnStatusChangedFrom(oldStatus);

        //}

        #region State transition pass-throughs

        protected override BehaviorStatus OnStart()
        {
            return Child.Start();
        }

        protected override void OnCanceling(string msg =null)
        {
            var childCopy = _child;
            if (childCopy != null)
            {
                childCopy.Cancel();
            }
            base.OnCanceling(msg);
        }

        #endregion
        
    }
}
