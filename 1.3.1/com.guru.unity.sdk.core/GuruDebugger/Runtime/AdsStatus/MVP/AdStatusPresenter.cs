

namespace Guru
{
    using System;
    using UnityEngine;
    
    public partial class AdStatusPresenter
    {
        const string K_ADS_STATUS_NODE = "ads_status";
        
        private AdStatusMonitorView _monitor;
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
            _monitor = monitorView;
            if (_monitor == null) _monitor = LoadDebuggerRoot();
            if (_monitor != null)
            {
                _monitor.OnUpdateInfo("ads is on loading...");
                _monitor.Active = false;
            }
            
            _monitor.OnEnableHandler = OnMonitorEnableEvent;
            _monitor.OnClickHandler = OnMonitorClickEvent;
            
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
                _monitor = t.GetComponent<AdStatusMonitorView>();
                return _monitor;
            }
            return null;
        }



        private void UpdateView()
        {
            if (_model == null) return;
            _monitor.OnUpdateInfo(_model.monitorInfo);
        }


        internal void ShowMonitor()
        {
            if (_monitor == null) return;
            _monitor.Active = true;
        }
        internal void HideMonitor()
        {
            if (_monitor == null) return;
            _monitor.Active = false;
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





    