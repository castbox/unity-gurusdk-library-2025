

namespace Guru
{
    using System;
    using UnityEngine;
    
    /// <summary>
    /// 广告单元 ID 配置 
    /// </summary>
    [Serializable]
    public class AdChannelUnitIds
    {
        public string bannerUnitID;	// Banner ID
        public string interUnitID;	// Inter ID
        public string rewardUnitID;	// Reward ID
    }
    
    [Serializable]
    /// <summary>
    /// 广告渠道配置
    /// </summary>
    public class AdChannelSettings
    {
        [SerializeField] private AdChannelUnitIds Android;
        [SerializeField] private AdChannelUnitIds iOS;
        
        /// <summary>
        /// 获取AppID
        /// </summary>
        /// <returns></returns>
        public AdChannelUnitIds UnitIds()
        {
#if UNITY_IOS
			return iOS; 
#else
            return Android;
#endif 
        }
        
        public string BannerUnitID => UnitIds().bannerUnitID;
        public string InterUnitID => UnitIds().interUnitID;
        public string RewardUnitID => UnitIds().rewardUnitID;
        
    }

}