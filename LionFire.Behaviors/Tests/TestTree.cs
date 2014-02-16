using LionFire.Coroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors.Tests
{
    public class TestTree : Sequencer
    {
        protected override IEnumerable<IBehavior> NewChildren
        {
            get 
            {
                //yield return new IntervalRepeater(
                //    new RandomCondition()
                //    {
                //        Name = "Random 30% every .1s",
                //        ProbabilityPercent = 30,
                //        RecurranceParameters = new RecurranceParameters()
                //        {
                //            Interval = TimeSpan.FromSeconds(0.1),
                //        }
                //    },
                //new RecurranceParameters
                //{
                //    Interval = TimeSpan.FromSeconds(5),
                //    StartImmediately = false,
                //})
                //{
                //    StopAfterSuccesses = 5,
                //};

                yield return new Repeater(
                    new RandomCondition()
                    {
                        Name = "Random 30% every .1s",
                        ProbabilityPercent = 30,
                        RecurranceParameters = new RecurranceParameters()
                        {
                            Interval = TimeSpan.FromSeconds(0.1),
                        }
                    }
                )
                {
                    StopAfterSuccesses = 5,
                };


                //yield return new OldPollingRepeater(
                //    new RandomCondition()
                //{
                //    Name = "Random 10% every .1s",
                //    ProbabilityPercent = 20,
                //    RecurranceParameters = new RecurranceParameters()
                //    {
                //        Interval = TimeSpan.FromSeconds(0.1),
                //    }
                //},
                //new RecurranceParameters
                //{
                //    Interval = TimeSpan.FromSeconds(3),
                //    StartImmediately = false,

                //})
                //{
                //    StopAfterSuccesses = 3,
                //};

                //yield return new SuccessBehavior() { Name = "Success1" };
                //yield return new SuccessBehavior() { Name = "Success2" };

                yield return new DelayedCondition(TimeSpan.FromSeconds(3), BehaviorStatus.Succeeded) { Name = "Succeed3s" };
                yield return new DelayedCondition(TimeSpan.FromSeconds(3), BehaviorStatus.Failed) { Name = "Fail3s", FailMessage = "Test fail" };

                //yield return new RandomCondition()
                //{
                //    Name = "Random 20% every 5s",
                //    ProbabilityPercent = 20,
                //    RecurranceParameters = new RecurranceParameters()
                //    {
                //        StartImmediately = false,
                //        Interval = TimeSpan.FromSeconds(5),
                //    }
                //};

                //yield return new RandomCondition()
                //{
                //    Name = "Random 20% every .5s",
                //    ProbabilityPercent = 20,
                //    RecurranceParameters = new RecurranceParameters()
                //    {
                //        Interval = TimeSpan.FromSeconds(0.5),
                //        StartImmediately = true,
                //    }
                //};
                //yield return new RandomCondition()
                //{
                //    Name = "Random 10% every .1s",
                //    ProbabilityPercent = 20,
                //    RecurranceParameters = new RecurranceParameters()
                //    {
                //        Interval = TimeSpan.FromSeconds(0.1),
                //    }
                //};

                //yield return new RandomCondition()
                //{
                //    Name = "Random 3% every .01s",
                //    ProbabilityPercent = 3,
                //    RecurranceParameters = new RecurranceParameters()
                //    {
                //        Interval = TimeSpan.FromSeconds(0.01),
                //    }
                //};

                //yield return new FailureBehavior() { Name = "InstantFail" };


                yield return new SuccessBehavior() { Name = "Final Success" };
            }
        }
    }
}
