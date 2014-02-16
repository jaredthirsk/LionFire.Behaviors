using LionFire.Coroutines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public interface IPoller : IHasRecurranceParameters
#if !NoPollerCoroutines
, IEnumerator
#endif
    {
#if NoPollerCoroutines
        bool Poll();
#endif
    }
}
