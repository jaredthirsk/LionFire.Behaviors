using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public interface IMultiComposite : ICollection<IBehavior>, IEnumerable
    {
    }
    public abstract class MultiComposite : Composite, IMultiComposite
    {
        protected List<IBehavior> children;

        public sealed override IEnumerable<IBehavior> Children
        {
            get
            {
                if (children != null)
                {
                    foreach (var child in children) yield return child;
                }
            }
        }

        protected override IEnumerable<IBehavior> NewChildren
        {
            get { yield break; }
        }

        protected sealed override void CreateChildrenIfNeeded()
        {
            if (children == null)
            {
                var kids = GetNewChildrenWithContext;
                children = new List<IBehavior>(kids);
                foreach (var child in children)
                {
                    OnChildAdded(child);
                }
            }
        }
        protected sealed override void ClearChildren()
        {

            if (children != null)
            {

                foreach (var child in children)
                {
                    child.Cancel();
#if BEHAVIOR_DISPOSE
                    child.Dispose();
#endif
                }
            }
            children = null;
        }

        protected sealed override void CancelChildren()
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    child.Cancel();
                }
            }
        }
        

        #region ICollection<IBehavior> Implementation

        public void Add(IBehavior behavior)
        {
            if (IsReadOnly) throw new NotSupportedException("ReadOnly");
            CreateChildrenIfNeeded();

            // REVIEW - remove this IDecorator stuff? too complicated?
            IDecorated d;
            for (d = behavior as IDecorated; d != null && d.Decorator != null; d = behavior as IDecorated)
            {
                behavior = d.Decorator;
            }

            children.Add(behavior);

            OnChildAdded(behavior);

            // It's up to derived classes to start children on add by adding this: (removing always cancels)
            //var status = Status;
            //switch (status)
            //{
            //    case BehaviorStatus.Running:
            //        behavior.Start();
            //        break;
            //    default:
            //        break;
            //}
        }

        public bool Remove(IBehavior behavior)
        {
            if (children == null) return false;
            if (IsReadOnly) throw new NotSupportedException("ReadOnly");

            bool result = children.Remove(behavior);
            if (result)
            {
                OnChildRemoved(behavior);

                //var status = Status;
                //switch (status)
                //{
                //    case BehaviorStatus.Running:
                //        behavior.Cancel();
                //        break;
                //    default:
                //        break;
                //}
            }
            return result;
        }

        public void Clear()
        {
            foreach (var child in children)
            {
                Remove(child);
            }
        }

        public bool Contains(IBehavior item)
        {
            if (children == null) return false;
            return children.Contains(item);
        }

        public void CopyTo(IBehavior[] array, int arrayIndex)
        {
            if (children == null) return;
            children.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { if (children == null) return 0; return children.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<IBehavior> GetEnumerator()
        {
            CreateChildrenIfNeeded();
            return children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
