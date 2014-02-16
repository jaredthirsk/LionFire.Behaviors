using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public class SelectorPersonality
    {
        #region Static

        public static readonly SelectorPersonality Default = new SelectorPersonality();

        #endregion

        public SelectorPersonality()
        {
        }

        public TimeSpan MinTimeBetweenChangeMind { get; set; }

        /// <summary>
        /// e.g. 0.3 to add 30% to score.  -0.3 to change mind (indecisive)
        /// 0 for neutral
        /// </summary>
        public float CurrentTaskBias { get; set; }        
    }

    
}
