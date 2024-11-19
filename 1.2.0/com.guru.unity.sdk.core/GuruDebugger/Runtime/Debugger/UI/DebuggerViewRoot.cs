



namespace Guru
{
    using UnityEngine.EventSystems;
    using UnityEngine;
    using UnityEngine.UI;
    using G = GlobalVars;
    
    using System;
    using System.Collections.Generic;
    
    public partial class DebuggerViewRoot: UIComponent, IViewFactory, IWidgetFactory
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private RectTransform _options;
        [SerializeField] private RectTransform _tabContent;
        [SerializeField] private RectTransform _optContent;
        [SerializeField] private RectTransform _binNode;
        
        
        [SerializeField] private Button _btnClose;
        [SerializeField] private AdStatusMonitorView _adStatusMoniter;
        
        private const string PrefabPath = "ui/debugger_root";
        private const string InstanceName = "__debugger__";
        
        private static DebuggerViewRoot _instance;
        public static DebuggerViewRoot Instance
        {
            get
            {
                if (_instance == null) _instance = CreateInstance();
                return _instance;
            }
        }
        
        private EventSystem _eventSystem;
        public AdStatusMonitorView AdStatusMonitor => _adStatusMoniter;
        public RectTransform Options => _options;

        #region Static Calls

        private static DebuggerViewRoot CreateInstance()
        {
            var p = Resources.Load<GameObject>(PrefabPath);
            if (p != null)
            {
                var go = Instantiate(p);
                DontDestroyOnLoad(go);
                go.name = InstanceName;
                return go.GetComponent<DebuggerViewRoot>();
            }
            return null;
        }




        #endregion

        #region Initialization

        private void Awake()
        {
            Init();
        }


        private void Init()
        {
            InitFactory();
            
            _btnClose.onClick.AddListener(OnCloseBtnEvent);

            if (EventSystem.current == null)
            {
                SetupEventSystem();
            }
            else
            {
                _eventSystem = EventSystem.current;
            }
            
            HideOptions();
        }

        private void SetupEventSystem()
        {

            var go = new GameObject(nameof(EventSystem));
            var es = go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            es.transform.parent = transform;
            _eventSystem = es;
        }

        private void OnCloseBtnEvent()
        {
            HideOptions();
            OnSelfClosed();
        }

        #endregion
        
        #region Display
        
        public void ShowOptions()
        {
            _options.gameObject.SetActive(true);
            if(_adStatusMoniter != null) 
                _adStatusMoniter.Active = false;
        }

        public void HideOptions()
        {
            _options.gameObject.SetActive(false);
            if(_adStatusMoniter != null) 
                _adStatusMoniter.Active = true;
        }

        #endregion

        #region Pages

        
        internal void RefreshTabs(string tanName, List<string> tabs = null)
        {
            if (tabs == null)
            {
                tabs = new List<string>(_displayedTabs.Count);
                foreach (var t in _displayedTabs)
                {
                    tabs.Add(t.Label);
                }
            }

            CleanTabs();
            foreach (var tn in tabs)
            {
                var tab = RegisterTab(tn);
                tab.Selected = tn == tanName;
                tab.OnClicked = OnTabClicked;
            }
        }

        #endregion

        #region Dispose

        private void OnSelfClosed()
        {
            
            G.Events.OnUIEvent?.Invoke(G.Events.EventViewClosed, null);
        }
        
        /// <summary>
        /// 回收资源
        /// </summary>
        public void Dispose()
        {
            CleanTabs();
            CleanOptions();
        }

        #endregion

        #region ADStauts

        
        public void ShowAdStatusMonitor()
        {
            HideOptions();
        }
        public void HideAdStatusMonitor()
        {
            ShowOptions();
        }
        

        #endregion
    }
}