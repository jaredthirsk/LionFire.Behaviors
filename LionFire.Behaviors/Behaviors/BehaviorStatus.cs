using System;

namespace LionFire.Behaviors
{
    public enum BehaviorStatus
    {
        /// <summary>
        /// Behavior is Invalid and may not be started until it is first Initialized.
        /// </summary>
        Uninitialized,

        /// <summary>
        /// Initialized, not started
        /// </summary>
        Initialized, 

        Running,
        Failed,
        
        Succeeded,

        Disposed,

        Suspended, // Keep state, allow resume (or fail)

        // Faulted, ??

        //Standby, // For Conditions and Scorers, indicates Monitoring enabled, but node not activated by a parent scorer
        
        //Unknown,
    }
}
