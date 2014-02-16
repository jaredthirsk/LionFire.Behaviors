#if TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors.Tests
{
    public class FailureBehavior : Behavior
    {
        protected override BehaviorStatus OnStart()
        {
            return BehaviorStatus.Failed;
        }
    }
}

#endif