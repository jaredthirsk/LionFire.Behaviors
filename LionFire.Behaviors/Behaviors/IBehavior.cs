//#define BEHAVIOR_DISPOSE - do this for project
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public interface IBehavior
        //: IEnableable

#if BEHAVIOR_INPC
        : INotifyPropertyChanged
#endif
#if BEHAVIOR_DISPOSE
       , IDisposable
#endif
    {
#if BEHAVIOR_NAME
        string Name { get; set; }
#endif
        //object Owner { get; set; }

        /// <summary>
        /// Can only be set once at creation time
        /// </summary>
        IBehavior Parent { get; set; }

        object Context { get; set;  }

        string StatusMessage { get; }

        #region Configuration

        bool AllowsSuspend { get; }

        #endregion

        BehaviorStatus Status { get; }
        event Action<IBehavior, BehaviorStatus, BehaviorStatus> StatusChangedForFromTo;

        bool IsFinished { get; }
        bool IsRunning { get; }

        /// <summary>
        /// After a behavior has completed, invoke this to reinitialize.  Sets state to Initialized.
        /// </summary>
        BehaviorStatus Initialize();

        /// <summary>
        /// Changes the state from Initialized to Running (or Success or Failure if such can be determined right away.)
        /// 
        /// User: can invoke this
        /// Selector: invokes this on selected behavior
        /// Decorator: Passes this on to child
        /// </summary>
        BehaviorStatus Start();
        void Suspend();
        void Resume();

        /// <summary>
        /// Force the status to failure
        /// 
        /// User: can invoke this
        /// Reactive Selector: can invoke this if another option becomes more attractive
        /// Parallel: Invokes if one child fails
        /// GetOneSuccess: Invokes if one child succeeds
        /// Decorator: Passes this on to child
        /// </summary>
        void Cancel(string message = null);

        void Deinitialize();

        
    }

    public static class IBehaviorExtensions
    {
        public static void TrySetContextFromParent(this IBehavior behavior)
        {
            for (IBehavior parent = behavior.Parent; parent != null && behavior.Context == null; parent = parent.Parent)
            {
                behavior.Context = parent.Context;
            }
        }
    }
   
}
