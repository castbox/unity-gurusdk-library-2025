namespace Guru
{
    using System;
    using UnityEngine;
    using Newtonsoft.Json;
    
    public abstract class RemoteConfigBase<T>: IRemoteConfig<T> where T : IRemoteConfig<T>
    {
        /// <summary>
        /// 配置是否可用
        /// </summary>
        public bool enable { get; set; } = true;
        
        [JsonIgnore]
        public Action<T> OnValueChanged { get; set; }
        /// <summary>
        /// 转为Json
        /// </summary>
        /// <returns></returns>
        public virtual string ToJson() => JsonParser.ToJson(this);
        
        
    }
}