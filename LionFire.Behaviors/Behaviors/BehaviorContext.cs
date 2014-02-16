#if NO_BEHAVIOR_PARENTS
using System;


namespace LionFire.Behaviors
{
    public class BehaviorContext
    {
        [ThreadStatic]
        public static object Context;
    }
}
#endif