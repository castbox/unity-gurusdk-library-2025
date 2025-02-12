namespace Guru
{
    using UnityEngine;
    using System;
    
    internal class BindableProperty<T>
    {
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (_value.Equals(value)) return;
                
                _value = value;
                OnValueChanged?.Invoke(value);
            }
        }
        public event Action<T> OnValueChanged;
        
        public BindableProperty(T initValue)
        {
            _value = initValue;
        }
    }
}