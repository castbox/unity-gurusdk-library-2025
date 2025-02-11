
namespace Guru
{
    using System;
    
    /// <summary>
    /// LT 数据提供器
    /// </summary>
    public interface ILTPropertyDataHolder
    {
        DateTime LastActiveTime { get; set; }
        int LT { get; set; }
    }
    
    /// <summary>
    /// LT 数据上报器 
    /// </summary>
    public interface ILTPropertyReporter
    {
        void ReportUserLT(int lt);
    }
    
    
    
}