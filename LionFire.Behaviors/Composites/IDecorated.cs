using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    /// <summary>
    /// Behaviors can use this to wrap themselves inside a parent (such as a StartPoller, for a RecurringTask)
    /// </summary>
    public interface IDecorated
    {
        IBehavior Decorator { get; }
    }
}
