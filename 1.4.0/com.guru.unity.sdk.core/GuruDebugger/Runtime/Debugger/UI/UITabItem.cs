

namespace Guru
{
    using UnityEngine;
    using System;
    using UnityEngine.UI;
    using G = GlobalVars;
    
    public class UITabItem: UIComponent
    {
        [SerializeField] private VButton _btn;

        private string _label;
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                _btn.Label = value;
            }
        }

        private bool _selected = false;

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                Refresh();
            }
        }
        public Action<string> OnClicked;
        public Action<UITabItem> OnRecycle;

        #region Init

        protected override void OnCreated()
        {
            _btn.OnClicked = OnTabClickedEvent;
        }


        private void OnTabClickedEvent()
        {
            Selected = !Selected;
            OnClicked?.Invoke(_label);
            Refresh();
        }


        public void InitWithData(long gid, string label = "")
        {
            GID = gid;
            name = $"tab_{GID}";
            _btn.name = "_btn";

            if (!string.IsNullOrEmpty(label))
            {
                Label = label;
            }

        }


        #endregion
        
        #region UI

        public override void Refresh()
        {
            _btn.Color = Selected? G.Colors.LightGreen: G.Colors.Gray;
        }

        #endregion

        #region Recycle


        public void Dispose()
        {
            OnClicked = null;
            OnRecycle?.Invoke(this);
        }


        #endregion
        
    }
}