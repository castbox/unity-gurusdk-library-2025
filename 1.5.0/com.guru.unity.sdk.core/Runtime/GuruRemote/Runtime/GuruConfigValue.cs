namespace Guru
{
    using System;
    using System.Linq;
    using UnityEngine;
    
    /// <summary>
    /// 配置值的数据结构
    /// </summary>
    public struct GuruConfigValue
    {
        
        private const string TAG = "[GuruConfigValue]";
        
        /// <summary>
        /// 配置值来源
        /// </summary>
        public ValueSource Source { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; }
        /// <summary>
        /// 配置值(字符串格式)
        /// </summary>
        public string Value { get; set; }

        #region 类型转换方法

        /// <summary>
        /// 获取字符串值
        /// </summary>
        public string AsString(string defaultValue = "") => 
            Value ?? defaultValue;

        /// <summary>
        /// 获取整型值
        /// </summary>
        public int AsInt(int defaultValue = 0) => 
            TryParseValue(int.TryParse, defaultValue);

        /// <summary>
        /// 获取长整型值
        /// </summary>
        public long AsLong(long defaultValue = 0L) => 
            TryParseValue(long.TryParse, defaultValue);

        /// <summary>
        /// 获取浮点型值
        /// </summary>
        public float AsFloat(float defaultValue = 0f) => 
            TryParseValue(float.TryParse, defaultValue);

        /// <summary>
        /// 获取双精度浮点值
        /// </summary>
        public double AsDouble(double defaultValue = 0d) => 
            TryParseValue(double.TryParse, defaultValue);

        /// <summary>
        /// 获取布尔值
        /// </summary>
        public bool AsBool(bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(Value)) return defaultValue;
            
            var lowercaseValue = Value.ToLower();
            
            // 检查true模式
            if (RemoteConfigModel.BOOL_TRUE_PATTERNS.Contains(lowercaseValue))
                return true;
            
            // 检查false模式
            if (RemoteConfigModel.BOOL_FALSE_PATTERNS.Contains(lowercaseValue))
                return false;
                
            return defaultValue;
        }

        /// <summary>
        /// 获取类型化的值
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="defaultValue">默认值</param>
        public T GetValue<T>(T defaultValue = default)
        {
            try
            {
                return Type.GetTypeCode(typeof(T)) switch
                {
                    TypeCode.String => (T)(object)AsString(),
                    TypeCode.Int32 => (T)(object)AsInt(),
                    TypeCode.Int64 => (T)(object)AsLong(),
                    TypeCode.Single => (T)(object)AsFloat(),
                    TypeCode.Double => (T)(object)AsDouble(),
                    TypeCode.Boolean => (T)(object)AsBool(),
                    _ => defaultValue
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"{TAG} 类型转换失败 '{Value}' => {typeof(T)}: {ex.Message}");
                return defaultValue;
            }
        }

        #endregion
        
        #region 辅助方法

        /// <summary>
        /// 尝试解析值
        /// </summary>
        private T TryParseValue<T>(TryParseHandler<T> parser, T defaultValue)
        {
            if (string.IsNullOrEmpty(Value))
                return defaultValue;

            return parser(Value, out T result) ? result : defaultValue;
        }

        /// <summary>
        /// 解析委托
        /// </summary>
        private delegate bool TryParseHandler<T>(string value, out T result);

        #endregion
        

        #region 重写方法

        public override string ToString() => 
            $"[Value: {Value}, Source: {Source}, LastUpdate: {LastUpdated:yyyy-MM-dd HH:mm:ss}]";

        public bool Equals(GuruConfigValue other) =>
            Value == other.Value && 
            Source == other.Source && 
            LastUpdated.Equals(other.LastUpdated);

        public override bool Equals(object obj) =>
            obj is GuruConfigValue other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Value, Source, LastUpdated);

        #endregion
    }
}