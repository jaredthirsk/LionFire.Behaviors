using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public interface IResource
    {
        object ResourceOwner { get; }
    }

    public interface IResourceConsumer
    {
        void OnGotResource(IResource r, bool gotIt);
    }

    public interface ISelector :  IResource
    {
        SelectorParameters SelectorParameters { get; }
        TimeSpan MinTimeBetweenChangeMind { get; }
    }

    /// <summary>
    /// A simple selector that executes each child in order until one succeeds.  
    /// 
    /// Q: If a child starts then fails, should the selector try to start the next child?
    /// </summary>
    public abstract class Selector : SelectingMultiComposite, ISelector
    {
        #region Parameters

        #region SelectorParameters

        public SelectorParameters SelectorParameters
        {
            get { return selectorParameters ?? SelectorParameters.Default; }
            set { selectorParameters = value; }
        } private SelectorParameters selectorParameters;

        #endregion

        #region MinTimeBetweenChangeMind

        /// <summary>
        /// Milliseconds
        /// </summary>
        public TimeSpan MinTimeBetweenChangeMind { get { return SelectorParameters.MinTimeBetweenChangeMind; } }

        #endregion

        #endregion

        #region State

        private DateTime DontReselectBefore = DateTime.MinValue;

        #endregion

        #region Transitions

        protected override void OnStatusChangedFrom(BehaviorStatus oldStatus)
        {
            base.OnStatusChangedFrom(oldStatus);

            switch (Status)
            {
                case BehaviorStatus.Running:
                    // Enable child monitoring while this node is running
                    MonitorChildScores = true; //superfluous, as it is done in OnStart
                    break;
                case BehaviorStatus.Failed:
                case BehaviorStatus.Succeeded:
                    MonitorChildScores = false;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Children

        protected override void OnChildAdded(IBehavior child)
        {
            if (MonitorChildScores)
            {
                IScorer scorer = child as IScorer;
                if (scorer != null)
                {
                    scorer.ScoreChangedForTo += OnScorerScoreChanged;
                }
            }
            base.OnChildAdded(child);
        }
        protected override void OnChildRemoved(IBehavior child)
        {
            if (MonitorChildScores)
            {
                IScorer scorer = child as IScorer;
                if (scorer != null)
                {
                    scorer.ScoreChangedForTo -= OnScorerScoreChanged;
                }
            }
            base.OnChildRemoved(child);
        }

        #endregion

        #region Monitor Scores

        protected bool MonitorChildScores
        {
            get
            {
                return monitorChildScores;
            }
            set
            {
                if (monitorChildScores == value) return;

                if (value)
                {
                    monitorChildScores = value;

                    foreach (var child in children)
                    {
                        IScorer scorer = child as IScorer;
                        if (scorer == null) continue;

                        // Formerly, Pollers attached/detached as necessary, but now it's explicit.
                        scorer.IsMonitoringScore = true;

                        if (value) 
                        {
                            scorer.ScoreChangedForTo += OnScorerScoreChanged;
                        }
                        else
                        {
                            scorer.ScoreChangedForTo -= OnScorerScoreChanged;
                        }
                    }
                    SelectedChild = GetHighestScore();
                }
                else
                {
                    SelectedChild = null;
                }
            }
        }private bool monitorChildScores;

        void OnScorerScoreChanged(IScorer scorer, float score)
        {
            l.Trace(scorer.ToString() + " score: " + score);

            // if before min change mind time, queue an update for when that time comes

            // OPTIMIZE - could cache the current highest score and scorer to see if the highest one dipped (in which case, recalculate highest from all), or if a non-highest one beat the current high score

            SelectedChild = GetHighestScore();
        }
        
        #endregion

        #region Determine Highest Score

        private const float DefaultScore = 0f;

        private IBehavior GetHighestScore()
        {
            float highest = float.MinValue;
            IBehavior bestChild = null;

            foreach (var child in Children)
            {
                IScorer scorer = child as IScorer;
                
                float score = scorer == null ? DefaultScore : scorer.Score;

                if (score > highest)
                {
                    bestChild = child;
                    highest = score;
                }
            }
            return bestChild;
        }

        #endregion

        #region Deferred Updates TODO TOTEST

        private void StartDeferredUpdate(TimeSpan timeToWait)
        {
            l.Debug("UNTESTED - StartDeferredUpdate");
            // TODO: Use coroutine instead of timer
            var timer = new Timer(DoDeferredUpdate, null, (int)timeToWait.TotalMilliseconds, Timeout.Infinite);      // DOUBLECAST       
        }
        private void DoDeferredUpdate(object state)
        {
            l.Debug("UNTESTED - DoDeferredUpdate " + this.ToString());
            var deferredUpdateChildCopy = deferredUpdateChild;
            deferredUpdateChild = null;

            SelectedChild = deferredUpdateChildCopy;

#if SanityChecks
            if (SelectedChild != deferredUpdateChildCopy)
            {
                l.Warn("DoDeferredUpdate failed to set child"); // Maybe timer was inaccurate and fired too fast?
            }
#endif
        }

        private IBehavior deferredUpdateChild;

        #endregion

        #region SelectedChild

        protected override bool OnSelectingChild(IBehavior value)
        {
            DateTime nowUtc = DateTime.UtcNow;

            TimeSpan delta = DontReselectBefore - nowUtc;
            if (delta > TimeSpan.Zero)
            {
                deferredUpdateChild = value;
                if (deferredUpdateChild != null)
                {
                    StartDeferredUpdate(delta);
                }
                return false;
            }
            else
            {
                DontReselectBefore = DateTime.UtcNow + MinTimeBetweenChangeMind;
            }
            return true;
            //return base.OnSelectingChild(value); // true
        }

        /// <summary>
        /// Potential fof
        /// </summary>
        /// <param name="behavior"></param>
        /// <param name="oldStatus"></param>
        /// <param name="status"></param>
        /// <remarks>
        /// Potential alternative behavior in derived classes:
        ///  - if one fails, try another until one succeeds
        ///  - run all children in order  (until one fails, or all succeed)
        ///  - use max completions, max failures, max successes parameters
        /// </remarks>
        protected override void OnSelectedStatusChangedForTo(IBehavior behavior, BehaviorStatus oldStatus, BehaviorStatus status)
        {
#if SanityChecks
            if (behavior != SelectedChild) return;
#endif
            switch (status)
            {
                //case BehaviorStatus.Uninitialized:
                //    break;
                case BehaviorStatus.Initialized:
                    // Do nothing -- we just selected it
                    break;
                case BehaviorStatus.Running:
                    // Do nothing -- we just started it
                    break;
                case BehaviorStatus.Failed:
                    Fail(behavior.StatusMessage);
                    break;
                case BehaviorStatus.Succeeded:
                    Succeed();
                    break;
                //case BehaviorStatus.Disposed:
                //    break;
                default:
                    l.Trace("UNEXPECTED Status for Selector.SelectedChild: " + behavior.ToString());
                    break;
            }
        }
        
        #endregion

        protected override BehaviorStatus OnStart()
        {
            MonitorChildScores = true; // Gives children a chance to set their score
            SelectedChild = GetHighestScore();

            if (SelectedChild == null)
            {
                return BehaviorStatus.Succeeded;
            }
            SelectedChild.Initialize();
            return SelectedChild.Start();
        }

        #region Misc

        private static ILogger l = Log.Get();

        #endregion
    }

    //public class Probability
    //{
    //}

    //public class BehaviorTask
    //{
    //    public Behavior CurrentNode { get; set; }
    //}

    /// <summary>
    /// Queries the score of all children and executes the one with the highest score
    /// 
    /// Use a Decorator to determine score?
    /// </summary>
    public abstract class PollingPrioritySelector : Selector
    {
    }

    ///// <summary>
    ///// Queries the score of all children and executes the one with the highest score.
    ///// Keeps listening to scores in case they change, in which case the running child may be
    ///// stopped and another child may be started
    ///// </summary>
    //public class ReactivePrioritySelector : Selector
    //{
    //}


}
