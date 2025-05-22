using System;

namespace Guru
{
    
    /// <summary>
    /// 记录 GuruSDK 本次 Session 信息的数据结构
    /// </summary>
    public class GuruSDKSessionInfo
    {
        /// <summary>
        /// 本次初始化 SDK 的时间
        /// </summary>
        public DateTime startTime;
        /// <summary>
        /// 从 Session start 到 SDK init 成功经过的时间（秒）
        /// </summary>
        public double boostDuration; 
        

    }
}