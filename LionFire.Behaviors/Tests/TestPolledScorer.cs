using LionFire.Coroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors.Tests
{
    public class TestPolledScorer<T> : PolledScorer<T>
        //, IPolledScorer
        where T : IBehavior, new()
    {
        public override void UpdateScore()
        {
            var time = DateTime.UtcNow;
            this.Score = time.Second;
        }

        public RecurranceParameters ScorerRecurranceParameters
        {
            get { return null; }
        }

        
        //public override float Score
        //{
        //    get { throw new NotImplementedException(); }
        //    set { sco}
        //} private float score;

        //protected override IEnumerable<IBehavior> NewChildren
        //{
        //    get {yield break; }
        //}
    }
}
