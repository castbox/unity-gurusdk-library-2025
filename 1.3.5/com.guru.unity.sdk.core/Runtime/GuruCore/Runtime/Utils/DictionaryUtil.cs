
namespace Guru
{
    using System.Collections.Generic;

    
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



    }
}