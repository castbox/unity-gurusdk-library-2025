


namespace Guru
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    public static class GuruSDKUtils
    {
        public static Color HexToColor(string hexString)
        {
            if(string.IsNullOrEmpty(hexString)) return Color.clear;
            
            var hex = hexString.Replace("#", "");
            if(hex.Length < 6) return Color.clear;
            
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = 255;
            if (hex.Length >= 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color(r, g, b, a);
        }
        
        public static Dictionary<string, object> MergeDictionary(Dictionary<string, object> source, Dictionary<string, object> other)
        {
            int len = source?.Count ?? 0 + other?.Count ?? 0;
            if (len == 0) len = 10;
            var newDict = new Dictionary<string, object>(len);
            if (source != null)
            {
                foreach (var k in source.Keys)
                {
                    newDict[k] = source[k];
                }
            }
            
            if (other != null)
            {
                foreach (var k in other.Keys)
                {
                    newDict[k] = other[k];
                }
            }
            return newDict;
        }
        
        #region Dictionary 取值

        // ---------- 获取属性 ----------
        
        public static string GetString(this Dictionary<string, object> dict, string key, string defaultValue = "")
        {
            return GetValue<string>(dict, key, defaultValue);
        }
        public static bool GetBool(this Dictionary<string, object> dict, string key, bool defaultValue = false)
        {
            return GetValue<bool>(dict, key, defaultValue);
        }
        public static int GetInt(this Dictionary<string, object> dict, string key, int defaultValue = 0)
        {
            return GetValue<int>(dict, key, defaultValue);
        }
        public static double GetDouble(this Dictionary<string, object> dict, string key, double defaultValue = 0.0)
        {
            return GetValue<double>(dict, key, defaultValue);
        }

        /// <summary>
        /// 泛型获取属性值
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetValue<T>(Dictionary<string, object> dict, string key, T defaultValue)
        {
            try
            {
                if (dict.TryGetValue(key, out var value))
                {
                    return (T)value;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"$Get property from {key} failed, Exception:{ex}");
            }
            return defaultValue;
        }
        

        #endregion
        
        
    }
}