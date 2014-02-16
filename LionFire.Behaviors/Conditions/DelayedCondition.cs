using LionFire.Coroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    
    /// <summary>
    /// Can be injected in a Sequence to be used as a sleep
    /// </summary>
    public class DelayedCondition : PollingCondition, IPolledStatus
        , IHasStatusRecurranceParameters
    {
        DateTime startTime;
        //TimeSpan delay = TimeSpan.FromSeconds(10);
        TimeSpan Delay;
        TimeSpan delayRemaining { get { return RecurranceParameters.Interval; } set { recurranceParameters.Interval = value; } }
        BehaviorStatus result = BehaviorStatus.Succeeded;
        public string FailMessage;
        
        #region Construction

        public DelayedCondition() { }

        public DelayedCondition(TimeSpan delay, BehaviorStatus result)
        {
            switch (result)
            {
                case BehaviorStatus.Failed:
                case BehaviorStatus.Succeeded:
                    break;
                default:
                    throw new ArgumentException("result must be Succeeded or Failed");
            }
            this.Delay = delay;
            //this.delay = delay;
             
            this.result = result;
        }

        #endregion

        #region Transitions

        protected override BehaviorStatus OnStart()
        {
            startTime = DateTime.UtcNow;
            return base.OnStart();
        }

        protected override bool OnInitializing()
        {
            delayRemaining = Delay;
            return base.OnInitializing();
        }

        #endregion

        #region Poll for Status
                
        public override void UpdateStatus()
        {
            if (Status == BehaviorStatus.Running)
            {
                var time = DateTime.UtcNow;

                var remaining = (Delay - (time - startTime));
                l.Trace("DelayedCondition UpdateStatus() " + remaining.TotalSeconds + " remaining.");

                if (time - startTime > Delay)
                {
                    if (result == BehaviorStatus.Succeeded)
                    {
                        Succeed();
                    }
                    else if (result == BehaviorStatus.Failed)
                    {
                        Fail(FailMessage);
                    }
                    else throw new UnreachableCodeException();
                }
                else
                {
                    delayRemaining = remaining;
                }
            }
        }

        #endregion
        
        public RecurranceParameters RecurranceParameters
        {
            get
            {
                return recurranceParameters;
            } 
        } private RecurranceParameters recurranceParameters = new RecurranceParameters();

        #region Misc

        private static ILogger l = Log.Get();
		
        #endregion
    }
}
