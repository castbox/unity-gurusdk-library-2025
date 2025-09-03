#nullable enable
using System;
using System.Globalization;
using System.Collections.Generic;

namespace Guru
{
    
    public static class DictionaryUtil
    {

        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T TryGetValue<T>(Dictionary<string, object> dict, string key, T defaultValue = default(T))
        {
            if (dict.TryGetValue(key, out object value))
            {
                return (T)value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 合并字典
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Dictionary<string, object> Merge(Dictionary<string, object> a, Dictionary<string, object> b)
        {
            if(a == null && b == null) return null;
            if(b == null) return a;
            if(a == null) return b;

            var res = new Dictionary<string, object>();
            foreach (var key in a.Keys)
            {
                res[key] = a[key];
            }
            foreach (var key in b.Keys)
            {
                res[key] = b[key];
            }
            
            return res;
        }


        public static Dictionary<string, string> ToStringDictionary(Dictionary<string, object>? dict)
        {
            var result = new Dictionary<string, string>();
            if(dict == null) return result;

            // 数据转换
            foreach (var kvp in dict)
            {
                if (kvp.Value != null)
                {
                    // 这里需要判断一下 kvp.Value 的类型，
                    // 所有 int, float, double, decimal 等数值类型或者带有精度的值类型，在转化字符串的时候均需要加入 CultureInfo.InvariantCulture
                    // 确保转换不受设备语言或文本格式限制， 小数点必须为 '.'
                    result[kvp.Key] = kvp.Value switch
                    {
                        IConvertible convertible => convertible.ToString(CultureInfo.InvariantCulture),
                        _ => kvp.Value.ToString()
                    };
                    
                }
            }
            return result;
        }
    }
}