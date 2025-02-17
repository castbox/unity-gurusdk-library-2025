

namespace Guru
{
    using UnityEngine;
    
    public class AdsInitSpec
    {
        public bool loadBanner;
        public bool loadInterstitial;
        public bool loadRewarded;
        public bool autoLoad;
        public bool isDebug;
        public string bannerColorHex;
        
        public static AdsInitSpec Build(
            bool loadBanner = true, 
            bool loadInterstitial = true, 
            bool loadReward = true,
            bool autoLoad = true, 
            bool isDebug = false, 
            string bannerColorHex = "") 
        {
            return new AdsInitSpec
            {
                loadBanner = loadBanner,
                loadInterstitial = loadInterstitial,
                loadRewarded = loadReward,
                autoLoad = autoLoad,
                isDebug = isDebug,
                bannerColorHex = bannerColorHex
            };
        }
        
        public static AdsInitSpec BuildDefault(bool autoLoad = true, bool isDebug = false)
        {
            return Build(true, true, true, autoLoad, isDebug);
        }
        
        public static AdsInitSpec BuildWithNoAds(bool autoLoad = true, bool isDebug = false)
        {
            return Build(false, false, true, autoLoad, isDebug);
        }

    }
}