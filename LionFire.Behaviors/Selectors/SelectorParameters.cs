using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public class SelectorParameters
    {
        #region Static

        public static readonly SelectorParameters Default = new SelectorParameters();

        #endregion

        #region AllowChangeMind

        /// <summary>
        /// If false, don't interrupt a child until it succeeds or fails.  (Performance tip: stops listening to scores on alternates while a child is executing.  FUTURE: Could also discard other children)
        /// </summary>
        [DefaultValue(true)]
        public bool AllowChangeMind
        {
            get { return allowChangeMind; }
            set { allowChangeMind = value; }
        } private bool allowChangeMind = true;

        #endregion

        #region MinTimeBetweenChangeMind

        /// <summary>
        /// If 0, there is no minimum.
        /// If NaN, fall back to Personality's value
        /// </summary>
        //[DefaultValue(TimeSpan.MinValue)]
        public TimeSpan MinTimeBetweenChangeMind
        {
            get { return minTimeBetweenChangeMind != TimeSpan.MinValue ? minTimeBetweenChangeMind : SelectorPersonality.MinTimeBetweenChangeMind; }
            set { minTimeBetweenChangeMind = value; }
        } private TimeSpan minTimeBetweenChangeMind = TimeSpan.MinValue;

        #endregion

        

        #region SelectorPersonality

        public SelectorPersonality SelectorPersonality
        {
            get { return selectorPersonality ?? SelectorPersonality.Default; }
            set { selectorPersonality = value; }
        } private SelectorPersonality selectorPersonality;

        #endregion
    }
}
