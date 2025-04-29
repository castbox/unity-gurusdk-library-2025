



namespace Guru
{
    using System;
    using Newtonsoft.Json;
    using UnityEngine;
    
    public static class JsonParser
    {
        /// <summary>
        /// 判断是否是合法的JSON
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static bool IsValidJson(string jsonStr)
        {
            try
            {
                if (!string.IsNullOrEmpty(jsonStr))
                {
                    if (jsonStr.TrimStart().StartsWith("{") && jsonStr.TrimEnd().EndsWith("}"))
                    {
                        return true;
                    }
                
                    if (jsonStr.TrimStart().StartsWith("[") && jsonStr.TrimEnd().EndsWith("]"))
                    {
                        return true;
                    } 
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ToObject<T>(string jsonStr)
        {
            if (IsValidJson(jsonStr))
            {
                try
                {
                    // return JsonMapper.ToObject<T>(jsonStr);
                    return JsonConvert.DeserializeObject<T>(jsonStr, new JsonSerializerSettings()
                    {
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                    });
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
            return default(T);
        }
        
        /// <summary>
        /// 转化为JSON字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="prettyFormat"></param>
        /// <returns></returns>
        public static string ToJson(object obj, bool prettyFormat = false)
        {
            try
            {
                /*
                if (!prettyFormat)
                {
                    return JsonMapper.ToJson(obj);
                }
                else
                {
                    JsonWriter writer = new JsonWriter()
                    {
                        IndentValue = 2,
                        PrettyPrint = true,
                    };
                    JsonMapper.ToJson(obj, writer);
                    return writer.ToString();
                }
                */
                
                var formatting = prettyFormat ? Formatting.Indented : Formatting.None;
                return JsonConvert.SerializeObject(obj, formatting);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }

            return "";
        }
    }
}