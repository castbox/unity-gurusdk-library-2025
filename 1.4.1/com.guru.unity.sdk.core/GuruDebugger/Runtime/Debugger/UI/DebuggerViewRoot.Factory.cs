



namespace Guru
{
    using System;
    using UnityEngine;
    using G = GlobalVars;
    using System.Collections.Generic;
    
    public partial class DebuggerViewRoot
    {
        
        [SerializeField] private UITabItem _tabPrefab;
        [SerializeField] private UIOptionItem _optPrefab;
        [SerializeField] private VButton _btnPrefab;
        [SerializeField] private VLabel _labelPrefab;
        
        private Queue<UITabItem> _tabPools;
        private Queue<UIOptionItem> _optPools;
        
        private List<UITabItem> _displayedTabs;
        private List<UIOptionItem> _displayedOptions;

        private long _tabIds = 0;
        private long _optIds = 0;
        private long _btnIds = 0;
        private long _lbIds = 0;

        private void InitFactory()
        {
            _tabPools = new Queue<UITabItem>(10);
            _optPools = new Queue<UIOptionItem>(20);

            _tabPrefab.Parent = _binNode;
            _optPrefab.Parent = _binNode;


            _displayedTabs = new List<UITabItem>(10);
            _displayedOptions = new List<UIOptionItem>(20);
        }
        
        #region Tabs
        
        public UITabItem RegisterTab(string tabNam)
        {
            var tab = GetTab(tabNam);
            _displayedTabs.Add(tab);
            return tab;
        }

        
        
        public UITabItem BuildTab(string tabName)
        {
            var go = Instantiate(_tabPrefab.gameObject, _tabContent);
            go.SetActive(true);
            var tab = go.GetComponent<UITabItem>();
            tab.InitWithData(_tabIds, tabName);
            tab.OnRecycle = OnTabRecycle;
            tab.OnClicked = OnTabClicked;
            _tabIds++;
            return tab;
        }

        private void OnTabClicked(string tabName)
        {
            G.Events.OnUIEvent?.Invoke(G.Events.EventTabClicked, tabName);
        }

        private void OnTabRecycle(UITabItem tab)
        {
            _displayedTabs.Remove(tab);
            tab.Parent = _binNode;
            tab.transform.localPosition = Vector3.zero;
            _tabPools.Enqueue(tab);
        }


        private UITabItem GetTab(string tabName)
        {
            if (_tabPools.Count > 0)
            {
                var tab = _tabPools.Dequeue();
                tab.Label = tabName;
                tab.Parent = _tabContent;
                return tab;
            }
            return BuildTab(tabName);
        }

        #endregion

        #region Options


        public UIOptionItem RegisterOption(string optName, string content = "")
        {
            var opt = GetOption(optName);
            opt.Content = content;
            opt.Refresh();
            _displayedOptions.Add(opt);
            return opt;
        }
        
        
        
        public UIOptionItem BuildOption(string optName)
        {
            var go = Instantiate(_optPrefab.gameObject, _optContent);
            go.SetActive(true);
            var opt = go.GetComponent<UIOptionItem>();
            opt.InitWithData(_optIds, optName);
            opt.OnRecycle = OnOptionRecycle;
            _optIds++;
            return opt;
        }

        
        private UIOptionItem GetOption(string name)
        {
            if (_optPools.Count > 0)
            {
                var opt = _optPools.Dequeue();
                opt.Label = name;
                opt.Parent = _optContent;
                return opt;
            }
            return BuildOption(name);
        }
        
        /// <summary>
        /// 选项回收
        /// </summary>
        /// <param name="opt"></param>
        private void OnOptionRecycle(UIOptionItem opt)
        {
            _displayedOptions.Remove(opt);   
            opt.Parent = _binNode;
            opt.transform.localPosition = Vector3.zero;
            _optPools.Enqueue(opt);
        }
      
        #endregion
        
        #region Button

        public VButton BuildButton(string name, Action onClick, Transform parent)
        {
            var go = Instantiate(_btnPrefab.gameObject, parent);
            var btn = go.GetComponent<VButton>();
            btn.Label = name;
            btn.OnClicked = onClick;
            btn.Size = new Vector2(120, 0);
            return btn;
        }
        
        #region 添加组件

        internal VButton AddOptionButton(UIOptionItem option, string btnName, Action btnHandler)
        {
            var btn = BuildButton(btnName, btnHandler, option.Root);
            option.AddChild(btn.gameObject);
            return btn;
        }
        internal VLabel AddOptionLabel(UIOptionItem option, string label, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var lb = BuildLabel(label, align, option.Root);
            option.AddChild(lb.gameObject);
            return lb;
        }



        #endregion
        
        #endregion
        
        #region Label

        public VLabel BuildLabel(string lbName, TextAnchor align, Transform parent)
        {
            var go = Instantiate(_labelPrefab.gameObject, parent);
            go.name = lbName;
            var label = go.GetComponent<VLabel>();
            label.Text = lbName;
            label.Align = align;
            label.Size = new Vector2(300, 0);
            return label;
        }

        #endregion

        #region Recycle
        
        internal void CleanTabs()
        {
            while (_displayedTabs.Count > 0)
            {
                var tab = _displayedTabs[0];
                tab.Dispose();
            }
        }



        internal void CleanOptions()
        {
            while (_displayedOptions.Count > 0)
            {
                var opt = _displayedOptions[0];
                opt.Dispose();
            }

        }

        #endregion
        
    }
}