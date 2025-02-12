

namespace Guru
{
    using System;
    
    public class AdStatus
    {

        private static AdStatusPresenter _adp;

        private static bool _inited;
        
        
        public static void Install(AdStatusMonitorView view = null, Action onMonitorClicked = null)
        {
            if (_inited) return;
            _inited = true;
            _adp = new AdStatusPresenter();
            _adp.Init(view, onMonitorClicked);
        }

        public static void ShowMonitor()
        {
            if(!_inited) Install();

            if (_adp != null)
            {
                _adp.ShowMonitor();
            }
        }

        public static void HideMonitor()
        {
            if(!_inited) Install();

            if (_adp != null)
            {
                _adp.HideMonitor();
            }
        }

    }
}