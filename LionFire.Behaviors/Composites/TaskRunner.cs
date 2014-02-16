#define LOG_TASK_FINISHED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    /// <summary>
    /// Run all children repeatedly.
    /// Suggestion: turn off default repeat time
    /// </summary>
    public  class TaskRunner : MultiComposite
    {
        #region finishedChildren
        
        List<IBehavior> finishedChildren ;

        private bool KeepFinishedTasks
        {
            get{return finishedChildren!=null;}
            set
            {
                if (value == KeepFinishedTasks) return;
                if (value) { finishedChildren = new List<IBehavior>(); } else { finishedChildren = null; }
            }
        }

        #endregion

        // OLD - use Default RP instead?
        ///// <summary>
        ///// If null, throw an exception when trying to run a child with no IHasStatusRecurranceParameters
        ///// </summary>
        //TimeSpan? DefaultInterval = null;

        protected override void OnChildStatusChanged(IBehavior child, BehaviorStatus oldStatus, BehaviorStatus newStatus)
        {
            if (child.IsFinished)
            {
                var finishedChildrenCopy = finishedChildren;
                if (finishedChildrenCopy != null)
                {
                    finishedChildrenCopy.Add(child);
                }
                this.children.Remove(child);

#if LOG_TASK_FINISHED
                l.Info("Task finished: " + child.ToString());
#endif
            }
            
            switch (newStatus)
            {
                case BehaviorStatus.Uninitialized:
                    break;
                case BehaviorStatus.Initialized:
                    break;
                case BehaviorStatus.Running:
                    break;
                case BehaviorStatus.Failed:
                    break;
                case BehaviorStatus.Succeeded:
                    break;
                case BehaviorStatus.Disposed:
                    break;
                default:
                    break;
            }
        }        

        protected override BehaviorStatus OnStart()
        {
            MonitoringChildren = true;

            foreach (var child in Children)
            {
                StartChild(child);
            }
            return BehaviorStatus.Running;
        }

        private void StartChild(IBehavior child)
        {
            child.Start();
            //IPolledStatus polledChild = child as IPolledStatus;

            //if (polledChild == null)
            //{
            //}
            //else
            //{
                
            //    RecurranceParameters rp = null;
            //    var hasRP = child as IHasStatusRecurranceParameters;
            //    if (hasRP != null)
            //    {
            //        rp = hasRP.StatusRecurranceParameters;
            //    }

            //    if (rp == null)
            //    {
            //        if (!DefaultInterval.HasValue)
            //        {
            //            throw new Exception("Children must have RecurranceParameters, or DefaultInterval must be set.");
            //        }
            //        else
            //        {
            //            rp = new RecurranceParameters() { Interval = DefaultInterval.Value };
            //        }
            //    }

            //    var poller = new StatusPoller(child, rp);
            //    poller.IsEnabled = true;
            //}
        }

        #region Misc

        private static ILogger l = Log.Get();
		
        #endregion
    }
}
