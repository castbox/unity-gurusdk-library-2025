namespace Guru.Ads.Max
{
    using System;
    using UnityEngine;
    using Guru.Ads;
    
    public class MaxRewardedLoader
    {
        private readonly ICustomAmazonLoader _adAmazonLoader;
        private readonly IAdEventObserver _eventObserver; // 广告事件监听器
        private readonly string _tag;
        private readonly MaxReviewCreativeIdCache _reviewCreativeIdCache;
        
        private string _maxAdUnitId;
        private DateTime _adStartLoadTime;
        private DateTime _adStartDisplayTime;
        private string _adPlacement;
        private int _retryCount;
        private bool _isAdLoading;
        
        public MaxRewardedLoader(string adUnitId, ICustomAmazonLoader adAmazonLoader, IAdEventObserver observer)
        {
            _maxAdUnitId = adUnitId;
            _adAmazonLoader = adAmazonLoader;
            _eventObserver = observer;
            _retryCount = 0;
            _isAdLoading = false;
            _tag = AdConst.LOG_TAG_MAX;
            _reviewCreativeIdCache = new MaxReviewCreativeIdCache();
            
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnAdsLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnAdDisplayFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdReviewCreativeIdGeneratedEvent += OnAdReviewCreativeIdGeneratedEvent;
        }
        
        /// <summary>
        /// 广告是否已经加载完成
        /// </summary>
        /// <returns></returns>
        public bool IsAdReady() => MaxSdk.IsRewardedAdReady(_maxAdUnitId);
        
        /// <summary>
        /// 更新广告 ID （测试用）
        /// </summary>
        /// <param name="adUnitId"></param>
        public void SetAdUnitId(string adUnitId) => _maxAdUnitId = adUnitId;
        
        /// <summary>
        /// 加载 Banner
        /// </summary>
        public void Load()
        {
            if (_isAdLoading)
            {
                Debug.Log($"{_tag} --- RADS Load skipped: isAdLoading...");
                return;
            }
            _adAmazonLoader.RequestRewarded(RequestMaxRewarded);
        }

        private void RequestMaxRewarded()
        {
            _isAdLoading = true;
            _adStartLoadTime = DateTime.UtcNow;
            // Amazon 预加载
            // MAX 加载广告
            MaxSdk.LoadRewardedAd(_maxAdUnitId);
            // 事件上报
            var evt = MaxAdEventBundleFactory.BuildRadsLoad(_maxAdUnitId, _adPlacement);
            _eventObserver.OnEventRadsLoad(evt);
            Debug.Log($"{_tag} --- RADS Load: { _maxAdUnitId}");
        }

        /// <summary>
        /// 显示 Banner
        /// </summary>
        /// <param name="placement"></param>
        public void Show(string placement = "")
        {
            if (!IsAdReady()) return;
            
            Debug.Log($"{_tag} --- RADS Show:: Placement { placement}");
            
            _adPlacement = placement;
            _adStartDisplayTime = DateTime.UtcNow;
            
            // 显示自动刷新
            MaxSdk.ShowRewardedAd(_maxAdUnitId);
        }
        

        /// <summary>
        /// 广告加载成功
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdsLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            // 加载成功
            var evt = MaxAdEventBundleFactory.BuildRadsLoaded(adUnitId, _adPlacement, _adStartLoadTime, adInfo);
            _eventObserver.OnEventRadsLoaded(evt);
            Debug.Log($"{_tag} --- RADS loaded {adUnitId} -> WaterfallName: {(adInfo.WaterfallInfo?.Name ?? "NULL")}  TestName: {(adInfo.WaterfallInfo?.TestName ?? "NULL")}");

            _retryCount = 0;
            _isAdLoading = false;
        }
        
        /// <summary>
        /// 广告展示成功
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnAdDisplayedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            // 广告显示
            // 2025-11-06 此点位改为由 paid 事件触发 -- By Yufei
            SendAdImpEvent(adUnitId, adInfo);
        }
        
        private void SendAdImpEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            var reviewCreativeId = _reviewCreativeIdCache.GetReviewedCreativeId(adInfo);
            var e = MaxAdEventBundleFactory.BuildRadsImp(adUnitId, adInfo, _adPlacement, reviewCreativeId);
            _eventObserver?.OnEventRadsImp(e);
            Debug.Log($"{_tag} --- RADS Imp: {adUnitId}");
        }
        
        /// <summary>
        /// 广告加载失败
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="errorInfo"></param>
        private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // 广告加载失败
            var reason = "load";
            var evt = MaxAdEventBundleFactory.BuildRadsFailed(adUnitId, reason, errorInfo, _adStartLoadTime);
            _eventObserver.OnEventRadsFailed(evt);

            _isAdLoading = false;
            MaxAdHelper.ReloadByRetryCount(_retryCount, ReloadAd).Forget();
            Debug.Log($"{_tag} --- RADS load Failed:: ErrorCode:{ errorInfo?.Code}  Info:{ errorInfo?.AdLoadFailureInfo ?? "Null" }");
        }
        /// <summary>
        /// 广告显示失败
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="errorInfo"></param>
        /// <param name="adInfo"></param>
        private void OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdk.AdInfo adInfo)
        {
            // 广告显示失败
            var reason = "imp";
            var evt = MaxAdEventBundleFactory.BuildRadsFailed(adUnitId, reason, errorInfo, _adStartDisplayTime);
            _eventObserver.OnEventRadsFailed(evt);
            MaxAdHelper.ReloadByRetryCount(_retryCount, ReloadAd).Forget();
            Debug.Log($"{_tag} --- RADS display failed:: ErrorCode:{ errorInfo?.Code }  Info:{ errorInfo?.AdLoadFailureInfo ?? "Null" }");
        }
        
        // 重新加载广告
        private void ReloadAd()
        {
            _retryCount++;
            Load();
        }
        
        
        /// <summary>
        /// 广告被点击
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdClickedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            if (string.IsNullOrEmpty(_adPlacement)) _adPlacement = AdConst.VALUE_NOT_SET;
            // 广告点击
            var reviewCreativeId = _reviewCreativeIdCache.GetReviewedCreativeId(adInfo);
            var evt = MaxAdEventBundleFactory.BuildRadsClick(adUnitId, adInfo, _adPlacement, reviewCreativeId);
            _eventObserver.OnEventRadsClick(evt);
            Debug.Log($"{_tag} --- RADS Clicked:: {_adPlacement}");
        }
        
        /// <summary>
        /// 广告收益
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            var reviewCreativeId = _reviewCreativeIdCache.GetReviewedCreativeId(adInfo);

            // 广告收益
            var evt = MaxAdEventBundleFactory.BuildRadsPaid(adUnitId, adInfo, _adPlacement, reviewCreativeId);
            _eventObserver.OnEventRadsPaid(evt);
            Debug.Log($"{_tag} --- RADS Paid:: Revenue:{adInfo.Revenue}");
            
            // 2025-11-06 由于 MAX 自身的主线程回归问题导致 IMP 时间延迟到 ADClose 之后才会调用，因此根据聪哥的建议，在 Paid 事件内触发 IMP 事件
            // 详见：https://www.tapd.cn/33527076/prong/stories/view/1133527076001022606?from_iteration_id=1133527076001002778
            // SendAdImpEvent(adUnitId, adInfo);
        }


        private void OnAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward, MaxSdk.AdInfo adInfo)
        {
            var reviewCreativeId = _reviewCreativeIdCache.GetReviewedCreativeId(adInfo);

            // 广告成功获得奖励
            var evt = MaxAdEventBundleFactory.BuildRadsRewarded(adUnitId, adInfo, _adPlacement, reviewCreativeId);
            _eventObserver.OnEventRadsRewarded(evt);
            Debug.Log($"{_tag} --- RADS Rewarded:{adInfo.Revenue}     Amount:{reward.Amount}");
        }

        private void OnAdHiddenEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            // 广告关闭
            var evt = MaxAdEventBundleFactory.BuildRadsClose(_maxAdUnitId, _adPlacement, _adStartDisplayTime);
            _eventObserver.OnEventRadsClose(evt);
            Debug.Log($"{_tag} --- RADS Close:{adUnitId}");
            if (_isAdLoading) _isAdLoading = false;
            
            // 延迟加载下一条广告
            // MaxAdHelper.DelayAction(AdConst.LOAD_NEXT_TIME, Load);
            Load();
        }


        /// <summary>
        /// 广告获取 reviewCreativeId
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="reviewCreativeId"></param>
        /// <param name="adInfo"></param>
        private void OnAdReviewCreativeIdGeneratedEvent(string adUnitId, string reviewCreativeId, MaxSdk.AdInfo adInfo)
        {
            Debug.Log($"{_tag} --- RADS get ReviewCreativeId:{reviewCreativeId}");
            _reviewCreativeIdCache.AddOrUpdate(adInfo, reviewCreativeId);
        }
        
    }
}