
using System;

namespace Guru
{
    using UnityEngine;
    using UnityEngine.UI;
    
    public class VLabel : UIComponent
    {
        [SerializeField] private Text _label;
        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }
        
        public Color Color
        {
            get => _label.color;
            set => _label.color = value;
        }

        private TextAnchor _align;
        public TextAnchor Align
        {
            get => _align;
            set
            {
                _align = value;
                _label.alignment = _align;
            }
        }

    }
}