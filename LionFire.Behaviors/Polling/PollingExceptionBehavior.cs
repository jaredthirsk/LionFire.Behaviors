using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    [Flags]
    public enum PollingExceptionBehavior
    {
        None = 0,
        Disable = 1 << 0,
        WrapAndRethrow = 1 << 1,
    }
}
