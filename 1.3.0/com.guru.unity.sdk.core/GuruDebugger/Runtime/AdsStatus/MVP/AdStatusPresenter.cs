

namespace Guru
{
    using System;
    using UnityEngine;
    
    public partial class AdStatusPresenter
    {
        const string K_DEBUGGER_ROOT = "ui/debugger_adstatus";
        
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
            var prefab = Resources.Load<GameObject>(K_DEBUGGER_ROOT);

            if (prefab != null)
            {
                var go = GameObject.Instantiate(prefab);
                go.name = "__debugger__";
                
                var t = go.transform.Find("root/ads_status_monitor");
                if (t != null)
                {
                    _monitor = t.GetComponent<AdStatusMonitorView>();
                    return _monitor;
                }
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





    