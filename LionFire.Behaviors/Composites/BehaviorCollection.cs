//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace LionFire.Behaviors
//{
    
//    /// <summary>
//    /// A MultiComposite that can have children added or removed after creation
//    /// </summary>
//    public abstract class BehaviorCollection : MultiComposite, ICollection<IBehavior>
//    {
//        protected  override IEnumerable<IBehavior> NewChildren
//        {
//            get { yield break; }
//        }
//        #region ICollection<IBehavior> Implementation

//        public void Add(IBehavior behavior)
//        {
//            CreateChildrenIfNeeded();

//            IDecorated d;
//            for (d = behavior as IDecorated; d != null && d.Decorator != null; d = behavior as IDecorated)
//            {
//                behavior = d.Decorator;
//            }

//            children.Add(behavior);
//            var status = Status;
//            switch (status)
//            {
//                case BehaviorStatus.Running:
//                    behavior.Start();
//                    break;
//                default:
//                    break;
//            }
//        }

//        public bool Remove(IBehavior behavior)
//        {
//            if (children == null) return false;

//            bool result = children.Remove(behavior);
//            if (result)
//            {
//                var status = Status;
//                switch (status)
//                {
//                    case BehaviorStatus.Running:
//                        behavior.Cancel();
//                        break;
//                    default:
//                        break;
//                }
//            }
//            return result;
//        }

//        public void Clear()
//        {
//            foreach (var child in children)
//            {
//                Remove(child);
//            }
//        }

//        public bool Contains(IBehavior item)
//        {
//            if (children == null) return false;
//            return children.Contains(item);
//        }

//        public void CopyTo(IBehavior[] array, int arrayIndex)
//        {
//            if (children == null) return;
//            children.CopyTo(array, arrayIndex);
//        }

//        public int Count
//        {
//            get { if (children == null) return 0; return children.Count; }
//        }

//        public bool IsReadOnly
//        {
//            get { return false; }
//        }

//        public IEnumerator<IBehavior> GetEnumerator()
//        {
//            CreateChildrenIfNeeded();
//            return children.GetEnumerator();
//        }

//        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
//        {
//            return this.GetEnumerator();
//        }

//        #endregion

//    }
//}
