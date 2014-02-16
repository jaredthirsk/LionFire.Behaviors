using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Behaviors
{
    public abstract class SelectingMultiComposite : MultiComposite, IResource
    {
        #region SuspendOnChangeMind

        /// <summary>
        /// When changing mind to a different child, suspend the one that was running
        /// </summary>
        public bool SuspendOnChangeMind
        {
            get { return suspendOnChangeMind; }
            set { suspendOnChangeMind = value; }
        } private bool suspendOnChangeMind;

        #endregion

        #region SelectedChild

        protected virtual bool OnSelectingChild(IBehavior value) { return true; }

        object IResource.ResourceOwner { get { return SelectedChild; } }

        public IBehavior SelectedChild
        {
            get { return selectedChild; }
            set
            {
                if (selectedChild == value) return;

                #region Consider Deferring

                if (!OnSelectingChild(value))
                {
                    return;
                }

                #endregion

                var oldSelectedChild = selectedChild;

                if (oldSelectedChild != null)
                {
                    oldSelectedChild.StatusChangedForFromTo -= OnSelectedStatusChangedForTo;
                    if (SuspendOnChangeMind)
                    {
                        oldSelectedChild.Suspend();
                    }
                    else
                    {
                        oldSelectedChild.Cancel();
                    }
                }

                selectedChild = value;
                
                if (selectedChild != null)
                {
                    if (selectedChild.IsFinished)
                    {
                        selectedChild.Initialize();
                    }

                    selectedChild.StatusChangedForFromTo += OnSelectedStatusChangedForTo;
                    OnSelectedStatusChangedForTo(selectedChild, selectedChild.Status, selectedChild.Status);
                    if (IsRunning) // redundant?
                    {
                        selectedChild.Start(); // or resume
                    }
                }

                #region Fire Changed Events

                //var ev = SelectedChildChangedForFrom;
                //if (ev != null) ev(this, oldSelectedChild);
                OnPropertyChanged("SelectedChild");

                #endregion

            }
        } private IBehavior selectedChild;

        //public event Action<MultiComposite, IBehavior> SelectedChildChangedForFrom;

        protected abstract void OnSelectedStatusChangedForTo(IBehavior behavior, BehaviorStatus from, BehaviorStatus status);


        #endregion

        protected override void OnFinished()
        {
            SelectedChild = null;
            base.OnFinished();
        }
    }
}
