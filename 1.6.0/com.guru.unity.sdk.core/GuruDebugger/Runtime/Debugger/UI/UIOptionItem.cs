

using System;
using System.Collections.Generic;

namespace Guru
{
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UIOptionItem: UIComponent
    {

        [SerializeField] private RectTransform _root;
        [SerializeField] private VLabel _label;
        [SerializeField] private VLabel _content;
        [SerializeField] private Image _bgImage;

        public Action<UIOptionItem> OnRecycle;
        private List<GameObject> _children;

        public Transform Root => _root;
        
        public string Label
        {
            get => _label.Text;
            set => _label.Text = value;
        }
        
        public string Content
        {
            get => _content.Text;
            set
            {
                _content.Text = value;
                _content.Active = !string.IsNullOrEmpty(value);
            }
        }


        private bool _clickable = false;
        public bool Clickable
        {
            get => _clickable;
            set
            {
                _clickable = value;
                _label.Active = !_clickable;
                _content.Active = !_clickable;
            }
        }


        public void InitWithData(long gid, string optName = "")
        {
            GID = gid;
            name = $"opt_{gid}";
            _children = new List<GameObject>(10);

            if (string.IsNullOrEmpty(optName))
            {
                _label.Active = false;
            }
            else
            {
                Label = optName;
            }

            _label.Align = TextAnchor.MiddleLeft;
            _content.Align = TextAnchor.MiddleLeft;

            
        }


        public void Dispose()
        {
            Clickable = false;
            ClearChildren();
            OnRecycle?.Invoke(this);
        }


        private void ClearChildren()
        {
            
            if(_children != null && _children.Count > 0)
            {
                foreach (var child in _children)
                {
                    Destroy(child);
                }
                _children.Clear();
            }
        }


        public void AddChild(GameObject obj)
        {
            _children.Add(obj);
        }


        public override void Refresh()
        {
            var idx = transform.GetSiblingIndex();
            _bgImage.color = idx % 2 == 0 ? GlobalVars.Colors.Gray : GlobalVars.Colors.Gray2;
        }
        



    }
}