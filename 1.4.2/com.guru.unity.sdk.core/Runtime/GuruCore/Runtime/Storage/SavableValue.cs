
using System;
using System.Collections;
using System.Text;

namespace Guru
{
    using UnityEngine;
    using System.Collections.Generic;

    
    public class SavableValue<T>: ISavableValue
    {
        private T _value;
        public T Value
        {
            get
            {
                if(null == _value) LoadValue();
                return _value;
            }

            set => SaveValue(value);
        }

        public Action<T> OnValueChanged;

        public string Key => nameof(T);


        #region 数据存储


        protected virtual void SaveValue(T value)
        {
            if (!_value.Equals(value))
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
            
            // ------------- 存储各种类型 ---------------
            if (_value is int)
            {
                SaveInt((int)(object)_value);
            }
            else if (_value is float)
            {
                SaveFloat((float)(object)_value);
            }
            else if (_value is string)
            {
                SaveString((string)(object)_value);
            }
            else if (_value is bool)
            {
                SaveBool((bool)(object)_value);
            }
            else if (_value is Vector2)
            {
                SaveVector2((Vector2)(object)_value);
            }
            else if (_value is Vector3)
            {
                SaveVector3((Vector3)(object)_value);
            }
            else if (_value is Vector4)
            {
                SaveVector4((Vector4)(object)_value);
            }
            else if (_value is Array)
            {
                SaveArray((string[])(object)_value);
            }
        }


        #endregion

        
        #region 数据读取

        protected virtual void LoadValue()
        {
            if (!HasKey()) _value = default; 
            
            if (_value is int)
            {
                _value = (T)(object)LoadInt();
            }
            else if (_value is float)
            {
                _value = (T)(object)LoadFloat();
            }
            else if (_value is string)
            {
                _value = (T)(object)LoadString();
            }
            else if (_value is bool)
            {
                _value = (T)(object)LoadBool();
            }
            else if (_value is Vector2)
            {
                _value = (T)(object)LoadVector2();
            }
            else if (_value is Vector3)
            {
                _value = (T)(object)LoadVector3();
            }
            else if (_value is Vector4)
            {
                _value = (T)(object)LoadVector4();
            }
            else if (_value is string[])
            {
                _value = (T)(object)LoadArray();
            }
        }


        #endregion


        #region 数据存储接口
        public void SaveInt(int value) => PlayerPrefs.SetInt(Key, value);
        public void SaveFloat(float value) => PlayerPrefs.SetString(Key, FloatToString(value));
        public void SaveBool(bool value) => PlayerPrefs.SetInt(Key, BoolToInt(value));
        public void SaveString(string value) => PlayerPrefs.SetString(Key, value);
        public void SaveVector2(Vector2 value) => PlayerPrefs.SetString(Key, Vector2ToString(value));
        public void SaveVector3(Vector3 value) => PlayerPrefs.SetString(Key, Vector3ToString(value));
        public void SaveVector4(Vector4 value) => PlayerPrefs.SetString(Key, Vector4ToString(value));
        public void SaveArray(string[] value) => PlayerPrefs.SetString(Key, ArrayToString(value));

        #endregion

        #region 数据加载接口

        
        public int LoadInt(int defaultValue = 0) => PlayerPrefs.GetInt(Key, 0);
        public float LoadFloat(float defaultValue = 0)
        {
            return StringToFloat(PlayerPrefs.GetString(Key, $"{defaultValue}"));
        }

        public bool LoadBool(bool defaultValue = false)
        {
            return IntToBool(PlayerPrefs.GetInt(Key, defaultValue?1: 0));
        }

        public string LoadString(string defaultValue = "")
        {
            return PlayerPrefs.GetString(Key, defaultValue);
        }
        
        public Vector2 LoadVector2(Vector2 defaultValue = new Vector2())
        {
            return StringToVector2(PlayerPrefs.GetString(Key, Vector2ToString(defaultValue)));
        }

        public Vector3 LoadVector3(Vector3 defaultValue = new Vector3())
        {
            return StringToVector3(PlayerPrefs.GetString(Key, Vector3ToString(defaultValue)));
        }
        
        public Vector4 LoadVector4(Vector4 defaultValue = new Vector4())
        {
            return StringToVector4(PlayerPrefs.GetString(Key, Vector4ToString(defaultValue)));  
        }

        public string[] LoadArray(string[] defaultValue = null)
        {
            if (!HasKey()) return defaultValue;
            return StringToArray(PlayerPrefs.GetString(Key,""));  
        }

        #endregion

        #region 操作接口

        public bool HasKey() => PlayerPrefs.HasKey(Key);


        public void ClearValue(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }


        


        #endregion

        #region 值转换

        private string FloatToString(float value) => value.ToString();
        private float StringToFloat(string value)
        {
            float val = (float)0.00;
            if (!string.IsNullOrEmpty(value))
            {
                float.TryParse(value, out val);
            }
            return val;
        }

        private int BoolToInt(bool value) => value ? 1 : 0;
        private bool IntToBool(int value) => value == 1;

        private string Vector2ToString(Vector2 value) => $"{value.x}_{value.y}";
        private Vector2 StringToVector2(string value)
        {
            if(value.IsNullOrEmpty()) return Vector2.zero;
            float x = 0;
            float y = 0;
            var raw = value.Split('_');
            if (raw.Length > 0) x = StringToFloat(raw[0]);
            if (raw.Length > 1) y = StringToFloat(raw[1]);
            return new Vector2(x, y);
        }
        
        private string Vector3ToString(Vector3 value) => $"{value.x}_{value.y}_{value.z}";
        private Vector3 StringToVector3(string value)
        {
            if(value.IsNullOrEmpty()) return Vector3.zero;
            float x = 0;
            float y = 0;
            float z = 0;
            var raw = value.Split('_');
            if (raw.Length > 0) x = StringToFloat(raw[0]);
            if (raw.Length > 1) y = StringToFloat(raw[1]);
            if (raw.Length > 2) z = StringToFloat(raw[2]);
            return new Vector3(x, y, z);
        }
        
        private string Vector4ToString(Vector4 value) => $"{value.x}_{value.y}_{value.z}_{value.w}";
        private Vector4 StringToVector4(string value)
        {
            if(value.IsNullOrEmpty()) return Vector2.zero;
            float x = 0;
            float y = 0;
            float z = 0;
            float w = 0;
            var raw = value.Split('_');
            if (raw.Length > 0) x = StringToFloat(raw[0]);
            if (raw.Length > 1) y = StringToFloat(raw[1]);
            if (raw.Length > 2) z = StringToFloat(raw[2]);
            if (raw.Length > 3) w = StringToFloat(raw[3]);
            return new Vector4(x, y, z, w);
        }

        private string ArrayToString(string[] value)
        {
            return  null == value ? "" : string.Join(",", value);
        }

        private string[] StringToArray(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var arr = value.Split(',');
            return arr;
        }
        
        
        
        #endregion

    }
    
    
    
    
    
}