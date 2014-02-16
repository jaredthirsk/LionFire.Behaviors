using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public interface ISequencer : IBehavior
    {
    }

    // TODO: Backtracking Sequencer: Does A B C D, monitors all, then if  B goes to Running again, it suspends (or cancels) D and goes back to B again.


    public class Sequencer : SelectingMultiComposite, ISequencer
    {
        #region CurrentChildIndex

        public int CurrentChildIndex
        {
            get { return currentChildIndex; }
            set { currentChildIndex = value; }
        } private int currentChildIndex = -1;

        #endregion

        private BehaviorStatus Advance()
        {
            IBehavior child;
            do
            {
                CurrentChildIndex++;
                if (children == null || CurrentChildIndex >= children.Count)
                {
#if SanityChecks
                    if (children.Count == 0)
                    {
                        l.Warn("Empty sequence succeeding. " + this.ToString());
                    }
#endif
                    return BehaviorStatus.Succeeded; // sets SelectedChild = null
                }

                child = children[CurrentChildIndex];
                
                BehaviorStatus status;

                status = child.Start();

                //l.Trace("Sequencer started child: " + child);
                l.Trace("Sequencer started child: " + status + " " + child);

            } while (child.Status == BehaviorStatus.Succeeded);

            SelectedChild = child; // Attaches to StatusChanged event (and fires it once in case it succeeded between here and a couple lines up)
            
            return child.Status; // Fail or Running
        }

        #region Transitions
        
        protected override BehaviorStatus OnStart()
        {
            CurrentChildIndex = -1;
            return Advance();
        }

        #endregion

        protected override void OnSelectedStatusChangedForTo(IBehavior child, BehaviorStatus oldStatus, BehaviorStatus status)
        {
#if SanityChecks
            if (child != SelectedChild)
            {
                l.Warn("child != SelectedChild");
                return;
            }
#endif

            switch (status)
            {
                //case BehaviorStatus.Uninitialized:
                //    break;
                //case BehaviorStatus.Initialized:
                //    break;
                case BehaviorStatus.Running:
                    // It is running as expected
                    break;
                case BehaviorStatus.Failed:
                    Fail(child.StatusMessage);
                    break;
                case BehaviorStatus.Succeeded:
                    if (Advance() == BehaviorStatus.Succeeded)
                    {
                        Succeed();
                    }
                    break;
                //case BehaviorStatus.Disposed:
                //    break;
                default:
                    l.Warn("UNEXPECTED Sequencer child state: " + child.ToString());
                    break;
            }
        }

        #region Misc

        private static ILogger l = Log.Get();
		
        #endregion

    }
}
