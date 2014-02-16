using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public abstract class Decorator : SingleComposite, IDecorator
    {
        #region Construction

        public Decorator() { }
        public Decorator(IBehavior child) : base(child) { }
        public Decorator(Func<IBehavior> childFactory) : base(childFactory)
        {
        }
        
        #endregion
    }

    public abstract class Decorator<T> : SingleComposite, IDecorator
        where T : IBehavior, new()
    {
        protected override Type ChildType
        {
            get { return typeof(T); }
        }

        #region Construction

        public Decorator() { }
        public Decorator(T child) : base(child) { }
        public Decorator(Func<IBehavior> childFactory)
            : base(childFactory)
        {
        }

        #endregion
    }


    public class DoOneThing : SingleComposite
    {
        public DoOneThing()
        {
            Start();
        }

        public void SetChild(IBehavior child)
        {
            base.Child = child;
        }

        protected override BehaviorStatus OnStart()
        {
            if (Child != null) Child.Start();

            return BehaviorStatus.Running;
        }
        protected override void OnChildStatusChanged(IBehavior child, BehaviorStatus from, BehaviorStatus newStatus)
        {
            var ev = ChildStatusChangedForParentAndChildFromTo;
            if (ev != null) ev(this, child,from,newStatus);

            if (child == this.Child)
            {
                if (child.IsFinished)
                {
                    base.Child = null;
                }
            }
            base.OnChildStatusChanged(child, from, newStatus);
        }

        public event Action<IBehavior, IBehavior, BehaviorStatus, BehaviorStatus> ChildStatusChangedForParentAndChildFromTo;
    }
}
