namespace Guru
{
    using System;
    
    /// <summary>
    /// 运控配置接口类
    /// </summary>
    public interface IRemoteConfig<T> where T : IRemoteConfig<T>
    {
        bool enable { get; set; }
        Action<T> OnValueChanged { get; set; } 
        string ToJson();
    }
}