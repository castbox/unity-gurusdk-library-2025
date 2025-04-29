namespace Guru.Ads.Max
{
    using System;
    using Guru.Ads;
    
    public static class MaxAdEventBundleFactory
    {
        
        private static int GetEventDuration(DateTime startTime)
        {
            return (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
        }

        private static string GetNotNullStringValue(string placement)
        {
            if(string.IsNullOrEmpty(placement)) return AdConst.VALUE_NOT_SET;
            return placement;
        }

        //----------------- BANNER --------------------
        public static BadsLoadEvent BuildBadsLoad(string adUnitId, string placement)
        {
            return new BadsLoadEvent(adUnitId, GetNotNullStringValue(placement));
        }
        public static BadsLoadedEvent BuildBadsLoaded(string adUnitId, string placement, DateTime startTime, MaxSdkBase.AdInfo adInfo)
        {
            return new BadsLoadedEvent(adUnitId, 
                GetNotNullStringValue(placement), 
                GetEventDuration(startTime), 
                adInfo.NetworkName,
                adInfo.NetworkPlacement, 
                GetNotNullStringValue(adInfo.WaterfallInfo?.Name ?? ""));
        }
        public static BadsFailedEvent BuildBadsFailed(string adUnitId, string placement, MaxSdk.ErrorInfo errorInfo, DateTime startTime)
        {
            return new BadsFailedEvent(adUnitId, 
                GetNotNullStringValue(placement), 
                (int)errorInfo.Code, 
                GetEventDuration(startTime),
                errorInfo.WaterfallInfo?.Name ?? "");
        }
        public static BadsImpEvent BuildBadsImp(string adUnitId, string placement)
        {
            return new BadsImpEvent(adUnitId, GetNotNullStringValue(placement));
        }
        public static BadsHideEvent BuildBadsHide(int loadedTimes, int failedTimes)
        {
            return new BadsHideEvent(loadedTimes, failedTimes);
        }
        public static BadsClickEvent BuildBadsClick(string adUnitId, string placement)
        {
            return new BadsClickEvent(adUnitId, GetNotNullStringValue(placement));
        }
        public static BadsPaidEvent BuildBadsPaid(string adUnitId, MaxSdkBase.AdInfo adInfo, string placement, string reviewedCreativeId)
        {
            return new BadsPaidEvent(adUnitId, 
                AdConst.CURRENCY_USD, 
                adInfo.Revenue,
                GetNotNullStringValue(adInfo.NetworkName),
                GetNotNullStringValue(adInfo.NetworkPlacement),
                GetNotNullStringValue(adInfo.CreativeIdentifier),
                GetNotNullStringValue(adInfo.AdFormat),
                GetNotNullStringValue(placement), 
                AdConst.AD_PLATFORM_MAX,
                reviewedCreativeId);
        }
        
        //----------------- INTERSTITIAL --------------------
        public static IadsLoadEvent BuildIadsLoad(string adUnitId, string placement)
        {
            return new IadsLoadEvent(adUnitId, GetNotNullStringValue(placement));
        }
        public static IadsLoadedEvent BuildIadsLoaded(string adUnitId, string placement, DateTime startTime, MaxSdk.AdInfo adInfo)
        {
            return new IadsLoadedEvent(adUnitId, 
                GetNotNullStringValue(placement), 
                GetEventDuration(startTime),
                GetNotNullStringValue(adInfo.NetworkName),
                GetNotNullStringValue(adInfo.NetworkPlacement),
                GetNotNullStringValue(adInfo.WaterfallInfo?.Name ?? ""));
        }
        public static IadsImpEvent BuildIadsImp(string adUnitId, MaxSdk.AdInfo adInfo, string placement, string reviewedCreativeId)
        {
            return new IadsImpEvent(adUnitId, 
                GetNotNullStringValue(adInfo.NetworkName),
                GetNotNullStringValue(adInfo.NetworkPlacement),
                GetNotNullStringValue(adInfo.CreativeIdentifier),
                GetNotNullStringValue(placement),
                AdConst.AD_PLATFORM_MAX,
                reviewedCreativeId);
        }
        public static IadsFailedEvent BuildIadsFailed(string adUnitId, string placement, MaxSdk.ErrorInfo errorInfo, DateTime startTime)
        {
            return new IadsFailedEvent(adUnitId, 
                GetNotNullStringValue(placement), 
                (int)errorInfo.Code, 
                GetEventDuration(startTime),
                GetNotNullStringValue(errorInfo.WaterfallInfo?.Name));
        }
        public static IadsClickEvent BuildIadsClick(string adUnitId, MaxSdk.AdInfo adInfo, string placement, string reviewedCreativeId)
        {
            return new IadsClickEvent(adUnitId,
                AdConst.CURRENCY_USD,
                adInfo.Revenue,
                GetNotNullStringValue(adInfo.NetworkName),
                GetNotNullStringValue(adInfo.NetworkPlacement),
                GetNotNullStringValue(adInfo.CreativeIdentifier),
                GetNotNullStringValue(placement),
                AdConst.AD_PLATFORM_MAX,
                reviewedCreativeId);
        }
        public static IadsCloseEvent BuildIadsClose(string adUnitId, string placement, DateTime startTime)
        {
            return new IadsCloseEvent(adUnitId,
                GetNotNullStringValue(placement), 
                GetEventDuration(startTime));
        }
        public static IadsPaidEvent BuildIadsPaid(string adUnitId, MaxSdkBase.AdInfo adInfo, string placement, string reviewedCreativeId)
        {
            return new IadsPaidEvent(adUnitId, 
                AdConst.CURRENCY_USD, 
                adInfo.Revenue,
                GetNotNullStringValue(adInfo.NetworkName),
                GetNotNullStringValue(adInfo.AdFormat),
                GetNotNullStringValue(adInfo.NetworkPlacement),
                GetNotNullStringValue(adInfo.CreativeIdentifier),
                GetNotNullStringValue(placement),
                AdConst.AD_PLATFORM_MAX,
                reviewedCreativeId);
        }
        
        //----------------- REWARDED --------------------
        public static RadsLoadEvent BuildRadsLoad(string adUnitId, string placement)
        {
            return new RadsLoadEvent(adUnitId, GetNotNullStringValue(placement));
        }
        public static RadsLoadedEvent BuildRadsLoaded(string adUnitId, string placement, DateTime startTime, MaxSdk.AdInfo adInfo)
        {
            return new RadsLoadedEvent(adUnitId, 
                GetNotNullStringValue(placement), 
                GetEventDuration(startTime),
                GetNotNullStringValue(adInfo.NetworkName),
                GetNotNullStringValue(adInfo.NetworkPlacement),
                GetNotNullStringValue(adInfo.WaterfallInfo?.Name));
        }
        public static RadsImpEvent BuildRadsImp(string adUnitId, MaxSdk.AdInfo adInfo, string placement, string reviewedCreativeId)
        {
            return new RadsImpEvent(adUnitId, 
                GetNotNullStringValue(adInfo.NetworkName),
                GetNotNullStringValue(adInfo.NetworkPlacement),
                GetNotNullStringValue(adInfo.CreativeIdentifier),
                GetNotNullStringValue(placement),
                AdConst.AD_PLATFORM_MAX,
                reviewedCreativeId);
        }
        public static RadsFailedEvent BuildRadsFailed(string adUnitId, string placement, MaxSdk.ErrorInfo errorInfo, DateTime startTime)
        {
            return new RadsFailedEvent(adUnitId, 
                GetNotNullStringValue(placement), 
                (int)errorInfo.Code, 
                GetEventDuration(startTime),
                errorInfo.WaterfallInfo?.Name ?? "");
        }
        public static RadsClickEvent BuildRadsClick(string adUnitId, MaxSdkBase.AdInfo adInfo, string placement, string reviewedCreativeId)
        {
            return new RadsClickEvent(adUnitId,
                AdConst.CURRENCY_USD,
                adInfo.Revenue,
                GetNotNullStringValue(adInfo.NetworkName),
                GetNotNullStringValue(adInfo.NetworkPlacement),
                GetNotNullStringValue(adInfo.CreativeIdentifier),
                placement,
                AdConst.AD_PLATFORM_MAX,
                reviewedCreativeId);
        }
        public static RadsCloseEvent BuildRadsClose(string adUnitId, string placement, DateTime startTime)
        {
            return new RadsCloseEvent(adUnitId,
                GetNotNullStringValue(placement), 
                GetEventDuration(startTime));
        }
        public static RadsPaidEvent BuildRadsPaid(string adUnitId, MaxSdkBase.AdInfo adInfo, string placement, string reviewedCreativeId)
        {
            return new RadsPaidEvent(adUnitId,
                AdConst.CURRENCY_USD,
                adInfo.Revenue,
                GetNotNullStringValue(adInfo.NetworkName),
                GetNotNullStringValue(adInfo.AdFormat),
                GetNotNullStringValue(adInfo.NetworkPlacement),
                GetNotNullStringValue(adInfo.CreativeIdentifier),
                GetNotNullStringValue(placement),
                AdConst.AD_PLATFORM_MAX,
                reviewedCreativeId);
        }
        public static RadsRewardedEvent BuildRadsRewarded(string adUnitId, MaxSdkBase.AdInfo adInfo, string placement, string reviewedCreativeId)
        {
            return new RadsRewardedEvent(adUnitId, 
                AdConst.CURRENCY_USD,
                adInfo.Revenue,
                GetNotNullStringValue(adInfo.NetworkName),
                GetNotNullStringValue(adInfo.NetworkPlacement),
                GetNotNullStringValue(adInfo.CreativeIdentifier),
                GetNotNullStringValue(placement),
                AdConst.AD_PLATFORM_MAX,
                reviewedCreativeId);
        }



    }
}