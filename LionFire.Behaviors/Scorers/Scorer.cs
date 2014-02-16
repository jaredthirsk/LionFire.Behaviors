#if TRACE
#define TRACE_SCORE
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    //public class OnDemandScorer // FUTURE
    //{
    //    ///// <summary>
    //    ///// Calculate on demand
    //    ///// 
    //    ///// Alternate ideas: GatedScoringDecorator - cache value for a certain time
    //    ///// </summary>
    //    //public abstract float Score { get; }
    //}

    /// <summary>
    /// Scorers have a dependency: a parent selector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ScorerBase<T> : Decorator<T>, IScorer
        where T : IBehavior, new()
    {
        public new T Child { get { return (T)base.Child; } set { base.Child = value; } }

        #region Construction

        public ScorerBase() { }
        public ScorerBase(T child) : base(child) { }
        public ScorerBase(Func<IBehavior> childFactory)
            : base(childFactory)
        {
        }

        #endregion

        public abstract float Score { get; protected set; }
        public abstract bool IsMonitoringScore { get; set; }

        #region ScoreChangedForTo

        //public abstract event Action<IScorer, float> ScoreChangedForTo;// {add; remove;}
        //protected abstract void RaiseScoreChanged();

        public event Action<IScorer, float> ScoreChangedForTo;

        protected void RaiseScoreChanged()
        {
            var ev = ScoreChangedForTo;
            if (ev != null) ev(this, Score);
        }

        #endregion

        #region Misc

#if TRACE
        private static ILogger l = Log.Get();
#endif

        #endregion
    }

    public abstract class Scorer<T> : ScorerBase<T>, IScorer // RENAME
        where T : IBehavior, new()
    {
        #region Construction

        public Scorer() { }
        public Scorer(T child) : base(child) { }
        public Scorer(Func<IBehavior> childFactory)
            : base(childFactory)
        {
        }

        #endregion

        #region Score

        public override float Score
        {
            get { return score; }
            protected set
            {
                if (score == value) return;
                score = value;

#if TRACE
                l.Trace(this.ToString() + " new score: " + value);
#endif

                RaiseScoreChanged();
#if BEHAVIOR_INPC
                OnPropertyChanged("Score");
#endif
            }
        } private float score;

        #endregion

        #region IsMonitoringScore

        public override bool IsMonitoringScore
        {
            get
            {
                return isMonitoringScore;
            }
            set
            {
                if (isMonitoringScore == value) return;
                isMonitoringScore = value;
                OnIsMonitoringScoreChangedTo(isMonitoringScore);
            }
        } bool isMonitoringScore;

        protected virtual void OnIsMonitoringScoreChangedTo(bool isMonitoringScore)
        {
#if TRACE
#if SanityChecks
            l.Warn("OnIsMonitoringScoreChangedTo not overridden, or base called.");
#endif
#endif
        }

        #endregion

        #region Misc

#if TRACE
        private static ILogger l = Log.Get();
#endif

        #endregion
    }

    //public  class Scorer<T> : ScorerBase<T>
    //    where T : IBehavior, new()
    //{
    //    #region Construction

    //    public Scorer() { }
    //    public Scorer(Func<IBehavior> childFactory) : base(childFactory) { }

    //    #endregion

    //    //#region ScoreChanged Event

    //    //protected override void RaiseScoreChanged()
    //    //{
    //    //    var ev = ScoreChangedForTo;
    //    //    if (ev != null) ev(this, Score);
    //    //}

    //    //public override event Action<IScorer, float> ScoreChangedForTo
    //    //{
    //    //    add
    //    //    {
    //    //        if (scoreChangedForTo == null)
    //    //        {
    //    //            UpdateScore(); // Update on first attach
    //    //        }
    //    //        scoreChangedForTo += value;
    //    //    }
    //    //    remove
    //    //    {
    //    //        scoreChangedForTo -= value;
    //    //    }
    //    //}
    //    //private event Action<IScorer, float> scoreChangedForTo;

    //    //#endregion

    //}

}
