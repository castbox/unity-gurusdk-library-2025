

namespace Guru
{
	using System.Collections.Generic;
	using UnityEngine;
	
    public partial class Analytics
    {
	    #region Ads-ATT

#if UNITY_IOS

	    /// <summary>
	    /// ATT 结果打点
	    /// </summary>
	    /// <param name="status"></param>
	    /// <param name="type"></param>
	    /// <param name="scene"></param>
	    public static void AttResult(string status, string type = "custom", string scene = "")
        {
	        SetAttProperty(status);
	        var dict = new Dictionary<string, dynamic>()
	        {
		        { ParameterItemCategory, status },
		        { "type", type }
	        };
	        if (!string.IsNullOrEmpty(scene))
		        dict[ParameterItemName] = scene;
	        
	        TrackEvent(TrackingEvent.Create(EventAttResult, dict));
        }

        /// <summary>
        /// 上报 ATT 当前的属性
        /// </summary>
        /// <param name="status"></param>
        private static void SetAttProperty(string status)
        {
	        Debug.Log($"{TAG} SetAttProperty: {status}");
            SetUserProperty(PropertyAttStatus, status);
        }
        
#endif

	    #endregion
    }
    
    
    
}