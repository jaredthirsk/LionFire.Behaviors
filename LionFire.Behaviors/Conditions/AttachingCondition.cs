using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    /// <summary>
    /// Typical scenario: evaluate whether condition is true right away.  If not, attach
    /// to events and wait for it.  Then detach.  
    /// </summary>
    public abstract class AttachingCondition : Condition
    {
        protected override BehaviorStatus OnStart()
        {
            Refresh();
            //bool result = TestCondition();
            //OnCondition(result);

            if (!IsFinished) // THREADSAFETY.  If condition got flipped between here and above we will miss it
            {
                IsAttached = true;
                return BehaviorStatus.Running;
            }
            else
            {
                return Status;
            }
        }

        public void Refresh()
        {
            OnCondition(TestCondition());
        }

        private void OnCondition(bool state)
        {
            //bool isDone = state;
                //(state && DoneWhenTestsTrue) || (!state && !DoneWhenTestsTrue);

            if (state)
            //{
                if (SucceedWhenDone)
                //{
                    Succeed();
                //}
                else
                //{
                    Fail();
                //}
                //return false;
            //}
            //return true;
        }
        
        public abstract bool TestCondition();

        protected override void OnFinished()
        {
            IsAttached = false;
            base.OnFinished();
        }

        protected abstract bool IsAttached
        {
            set;
        }
    }
}
