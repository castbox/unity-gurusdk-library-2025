

namespace Guru
{
    using System;
    using UnityEngine;
    
    public partial class AdStatusPresenter
    {
        const string K_ADS_STATUS_NODE = "ads_status";
        
        private AdStatusMonitorView _monitorView;
        private AdStatusModel _model;
        private Action _onMonitorClickedHandler;
        /// <summary>
        /// Initiallize
        /// </summary>
        /// <param name="monitorView"></param>
        public void Init(AdStatusMonitorView monitorView = null, Action onClicked = null)
        {
            _onMonitorClickedHandler = onClicked;
            
            _model = new AdStatusModel();
            _monitorView = monitorView;
            if (_monitorView == null) _monitorView = LoadDebuggerRoot();
            if (_monitorView != null)
            {
                _monitorView.OnUpdateInfo("ads is on loading...");
                _monitorView.Active = false;
            }
            
            _monitorView.OnEnableHandler = OnMonitorEnableEvent;
            _monitorView.OnClickHandler = OnMonitorClickEvent;
            
            InitAdsAssets();
        }

        /// <summary>
        /// Debugger Root
        /// </summary>
        private AdStatusMonitorView LoadDebuggerRoot()
        {
            var viewRoot = DebuggerViewRoot.Instance;
            var t = viewRoot.transform.Find(K_ADS_STATUS_NODE);
            if (t != null)
            {
                _monitorView = t.GetComponent<AdStatusMonitorView>();
                return _monitorView;
            }
            return null;
        }



        private void UpdateView()
        {
            if (_model == null) return;
            _monitorView.OnUpdateInfo(_model.monitorInfo);
        }


        internal void ShowMonitor()
        {
            if (_monitorView == null) return;
            _monitorView.Active = true;
        }
        internal void HideMonitor()
        {
            if (_monitorView == null) return;
            _monitorView.Active = false;
        }
        
        
        private void OnMonitorEnableEvent(bool enabled)
        {
            if (enabled)
            {
                RemoveCallbacks();
                AddCallbacks();
            }
            else
            {
                RemoveCallbacks();
            }
        }
        
        
        
        #region Click


        private void OnMonitorClickEvent()
        {
            _onMonitorClickedHandler?.Invoke();
        }



        #endregion
        
    }
}





    