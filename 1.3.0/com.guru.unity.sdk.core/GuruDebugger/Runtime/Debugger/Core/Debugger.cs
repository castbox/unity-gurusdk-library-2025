

namespace Guru
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using G = GlobalVars;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class GuruDebugger
    {
        public const string VERSION = "1.0.0";    
        private static bool _initOnce = false;
        private static GuruDebugger _instance;
        public static GuruDebugger Instance
        {
            get
            {
                if (_instance == null) _instance = new GuruDebugger();
                return _instance;
            }
        }

        public static event Action OnClosed
        {
            add => _onViewClosed += value;
            remove => _onViewClosed -= value;
        }

        private static Action _onViewClosed;

        private DebuggerViewRoot _viewRoot;
        private Dictionary<string, List<OptionLayout>> optionDicts;
        private string _curTabName;

        public delegate string GetOptionContentDelegate();
        
        private GuruDebugger()
        {
            if (_initOnce) return;
            _initOnce = true;
            StartService();
            
            Debug.Log($"[DEBUG] --- Guru Debugger [{VERSION}] start ---");
        }
        
        private void StartService()
        {
            _viewRoot = DebuggerViewRoot.Instance;
            optionDicts = new Dictionary<string, List<OptionLayout>>(5);
            G.Events.OnUIEvent += OnUIEvent;
            
            AdStatus.Install(_viewRoot.AdStatusMonitor, OnMonitorClicked);
        }

        /// <summary>
        /// 关闭视图
        /// </summary>
        public void Close()
        {
            OnUIEvent(G.Events.EventViewClosed);
        }
        
        #region UIEvent

        private void OnUIEvent(string evt, object data = null)
        {
            switch (evt)
            {
                case G.Events.EventTabClicked:
                    OnSelectTab(data.ToString());
                    break;
                case G.Events.EventViewClosed:
                    // optionDicts?.Clear();
                    _onViewClosed?.Invoke();
                    ShowAdStatus();
                    break;
                case G.Events.EventMonitorClicked:
                    HideAdStatus();
                    ShowPage();
                    break;
            }
        }
        
        
        private void OnMonitorClicked()
        {
            OnUIEvent(G.Events.EventMonitorClicked);
        }
        

        #endregion
        
        #region UI Layout
        
        /// <summary>
        /// 将传入的 uri 按照找到的第一个 '/' 字符，分割为 tabName 和 optName
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="tabName"></param>
        /// <param name="optName"></param>
        private void SplitOptionUri(string uri, out string tabName, out string optName)
        {
            tabName = G.Consts.DefaultTabName;
            optName = G.Consts.DefaultOptionName;
            var index = uri.IndexOf('/');
            if (index > -1)
            {
                tabName = uri.Substring(0, index);
                optName = (index + 1 < uri.Length) ? uri.Substring(index + 1): G.Consts.DefaultOptionName;
            }
            
            // if (uri.Contains("/"))
            // {
            //     var names = uri.Split('/');
            //     if (names.Length > 0) tabName = names[0];
            //     if (names.Length > 1) optName = names[1];
            // }
        }



        public OptionLayout AddOption(string uri, GetOptionContentDelegate contentDelegate, Action clickHandler = null)
        {
            SplitOptionUri(uri, out var tabName, out var optName);

            if (!optionDicts.ContainsKey(tabName))
            {
                optionDicts[tabName] = new List<OptionLayout>(10);
            }
  
            OptionLayout opt = new OptionLayout(tabName, optName, contentDelegate, clickHandler);
            AddOptionLayout(tabName, opt);
            
            return opt;
        }
        
        public OptionLayout AddOption(string uri, string content = "", Action clickHandler = null)
        {
            GetOptionContentDelegate del = null;
            if (!string.IsNullOrEmpty(content))
            {
                del = () => content;
            }
            return AddOption(uri, del, clickHandler);
        }

        private void AddOptionLayout(string tabName, OptionLayout layout)
        {
            TryToAddOption(tabName, layout);
        }

        private bool TryToAddOption(string tabName, OptionLayout opt)
        {
            if (!optionDicts.ContainsKey(tabName))
            {
                optionDicts[tabName] = new List<OptionLayout>(20);
                optionDicts[tabName].Add(opt);
                return true;
            }
            
            var options = optionDicts[tabName];
            var oldOpt = options.FirstOrDefault(c => c.IsEqual(opt));
            if (oldOpt == null)
            {
                options.Add(opt);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// 删除 URI
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public bool DeleteOption(string uri)
        {
            SplitOptionUri(uri, out var tabName, out var optName);
            return TryDeleteOption(tabName, optName);
        }

        private bool TryDeleteOption(string tabName, string optName)
        {
            if (optionDicts.TryGetValue(tabName, out var opts))
            {
                var o = opts.FirstOrDefault(c => c.optName == optName);
                if (o != null)
                {
                    opts.Remove(o);
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// 删除分页
        /// </summary>
        /// <param name="tabName"></param>
        /// <returns></returns>
        public bool DeleteTable(string tabName)
        {
            if (!optionDicts.ContainsKey(tabName)) return false;
            
            optionDicts.Remove(tabName);
            return true;
        }


        public void ShowAdStatus()
        {
            _viewRoot.ShowAdStatusMonitor();
        }
        
        
        public void HideAdStatus()
        {
            _viewRoot.HideAdStatusMonitor();
        }


        public void ShowPage(string tabName = "")
        {
            if (string.IsNullOrEmpty(tabName) 
                && optionDicts != null && optionDicts.Count > 0)
            {
                tabName = optionDicts.Keys.First();
            }

            if (!string.IsNullOrEmpty(tabName))
            {
                RenderPage(tabName);
            }
            
            
        }

        /// <summary>
        /// 渲染页面
        /// </summary>
        /// <param name="tabName"></param>
        private void RenderPage(string tabName)
        {
            if (string.IsNullOrEmpty(tabName)) return;
            
            _viewRoot.ShowOptions();
            _viewRoot.RefreshTabs(tabName, optionDicts.Keys.ToList());
            _viewRoot.CleanOptions();
            if (optionDicts.TryGetValue(tabName, out var opts))
            {
                OptionLayout ol;
                UIOptionItem ui;
                for (int i = 0; i < opts.Count; i++)
                {
                    ol = opts[i];
                    ui = _viewRoot.RegisterOption(ol.optName, ol.GetContent());

                    if (ol.selfClickHandler != null)
                    {
                        var btnName = ol.GetContent();
                        if (string.IsNullOrEmpty(btnName)) btnName = ol.optName;
                        var btn = _viewRoot.AddOptionButton(ui, btnName, ol.selfClickHandler);
                        ui.Clickable = true;
                        continue;
                    }
                    
                    foreach (var item in ol.items)
                    {
                        switch (item.type)
                        {
                            case "button":
                                var btn = _viewRoot.AddOptionButton(ui, item.name, item.clickHandler);
                                if (!item.size.Equals(Vector2.zero)) btn.Size = item.size;
                                break;
                            
                            case "label":
                                var lb = _viewRoot.AddOptionLabel(ui, item.name, item.align);
                                if (!item.size.Equals(Vector2.zero)) lb.Size = item.size;
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"Tab [{tabName}] not found!");
            }

        }


        private void OnSelectTab(string tabName)
        {
            if (_curTabName == tabName) return;
            Instance.ShowPage(tabName);
        }

        #endregion

        #region Display
        
        public static void Show(string tabName = "")
        {
            Instance.ShowPage(tabName);
        }

        public static void Hide()
        {
            Instance._viewRoot.HideOptions();
        }

        #endregion
        
        #region DebuggerOption
        
        public class OptionLayout
        {
            public string optName;
            public GetOptionContentDelegate contentDel;
            public string tabName;
            public Action selfClickHandler;

            internal List<OptionItemLayout> items;

            public string GetContent()
            {
                return contentDel != null ? contentDel() : "";
            }

            public OptionLayout(string tabName, string optName, GetOptionContentDelegate contentDel, Action selfClickHandler = null)
            {
                items = new List<OptionItemLayout>(10);
                this.tabName = tabName;
                this.optName = optName;
                this.contentDel = contentDel;
                this.selfClickHandler = selfClickHandler;
            }


            public OptionLayout AddLabel(string labelName)
            {
                items.Add(new OptionItemLayout()
                {
                    name = labelName,
                    type = "label",
                });
                return this;
            }
            
            public OptionLayout AddButton(string btnName, Action onClick)
            {
                items.Add(new OptionItemLayout()
                {
                    name = btnName,
                    type = "button",
                    clickHandler = onClick
                });
                return this;
            }
            
            public bool IsEqual(OptionLayout other)
            {
                var res = other.tabName == tabName
                          && other.optName == optName
                          // && other.content == content
                          && IsItemsEqual(other);
                return res;
            }


            private bool IsItemsEqual(OptionLayout other)
            {
                if (items == null && other.items == null) return true;
                if (items != null && other.items == null || items == null && other.items != null) return false;
                if (items != null && other.items != null)
                {
                    if (items.Count != other.items.Count)
                    {
                        return false;
                    }
                    
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (!items[i].IsEqual(other.items[i]))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        

        internal class OptionItemLayout
        {
            public string type;
            public Action clickHandler;
            public string name;
            public string content;
            public TextAnchor align = TextAnchor.MiddleCenter;
            public Vector2 size = Vector2.zero;
            
            public bool IsEqual(OptionItemLayout other)
            {
                return other.type == type 
                       && other.name == name; 
                       // && other.content == content;
            }
            
        }

        #endregion
        
    }


    public static class OptionLayoutExtension
    {
        public static GuruDebugger.OptionLayout AddCopyButton(this GuruDebugger.OptionLayout layout, Action onClick = null)
        {
            layout.AddButton("Copy", ()=>
            {
                string c = layout.contentDel?.Invoke() ?? "";
                GUIUtility.systemCopyBuffer = c;
                onClick?.Invoke();
            });
            return layout;
        }
    }



}