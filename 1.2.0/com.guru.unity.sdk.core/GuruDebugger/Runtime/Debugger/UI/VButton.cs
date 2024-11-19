

namespace Guru
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class VButton: UIComponent
    {
        [SerializeField] private Image _image;
        [SerializeField] private Text _label;
        [SerializeField] private Button _button;
        
        public string Label
        {
            get => _label.text;
            set => _label.text = value;
        }
        
        public Action OnClicked;
        
        public Color Color
        {
            get => _image.color;
            set => _image.color = value;
        }
        
        public Color LabelColor
        {
            get => _label.color;
            set => _label.color = value;
        }


        protected override void OnCreated()
        {
            _button.onClick.AddListener(OnSelfClicked);
        }
        private void OnSelfClicked()
        {
            OnClicked?.Invoke();
        }
        
        
        
        
        
        
        
    }
}