namespace Guru.Ads.Max
{
    using System;
    using UnityEngine;
    using Guru.Ads;
    
    public class MaxInterstitialLoader
    {
        private readonly ICustomAmazonLoader _customAmazonLoader;
        private readonly IAdEventObserver _eventObserver; // 广告事件监听器
        private readonly string _tag;
        
        private string _maxAdUnitId;
        private DateTime _adStartLoadTime;
        private DateTime _adStartDisplayTime;
        private string _adPlacement;
        private bool _isAdLoading;
        private int _retryCount;
        

        public MaxInterstitialLoader(string adUnitId, ICustomAmazonLoader customAmazonLoader, IAdEventObserver observer)
        {
            _maxAdUnitId = adUnitId;
            _customAmazonLoader = customAmazonLoader;
            _eventObserver = observer;
            _retryCount = 0;
            _isAdLoading = false;
            _tag = AdConst.LOG_TAG_MAX;
            
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnAdsLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnAdLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnAdDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnAdDisplayFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnAdClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdPaidEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnAdHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdReviewCreativeIdGeneratedEvent += OnAdsReviewCreativeIdGeneratedEvent;
        }

        public bool IsAdReady() => MaxSdk.IsInterstitialReady(_maxAdUnitId);
        
        public void SetAdUnitId(string adUnitId) => _maxAdUnitId = adUnitId;
        
        /// <summary>
        /// 加载 Banner
        /// </summary>
        public void Load()
        {
            if (_isAdLoading) return;
            _isAdLoading = true;
            Debug.Log($"{_tag} --- INTER Load: { _maxAdUnitId}");
            _customAmazonLoader.RequestInterstitial(CreateMaxInterstitial);
        }

        private void CreateMaxInterstitial()
        {
            _adStartLoadTime = DateTime.UtcNow;
            // Amazon 预加载
            // Max 加载广告
            MaxSdk.LoadInterstitial(_maxAdUnitId);
            // 广告加载
            var e = MaxAdEventBundleFactory.BuildIadsLoad(_maxAdUnitId, _adPlacement);
            _eventObserver?.OnEventIadsLoad(e);
        }

        /// <summary>
        /// 显示 Banner
        /// </summary>
        /// <param name="placement"></param>
        public void Show(string placement)
        {
            if (!IsAdReady()) return;
            
            Debug.Log($"{_tag} --- INTER Show:: Placement { placement}");
            
            _adPlacement = placement;
            _adStartDisplayTime = DateTime.UtcNow;
            
            // 显示自动刷新
            MaxSdk.ShowInterstitial(_maxAdUnitId);
        }
        

        /// <summary>
        /// 广告加载成功
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdsLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // 加载成功
            var e = MaxAdEventBundleFactory.BuildIadsLoaded(adUnitId, _adPlacement, _adStartLoadTime, adInfo);
            _eventObserver?.OnEventIadsLoaded(e);
            _retryCount = 0;
            _isAdLoading = false;
            Debug.Log($"{_tag} --- INTER loaded {adUnitId} -> WaterfallName: {(adInfo.WaterfallInfo?.Name ?? "NULL")}  TestName: {(adInfo.WaterfallInfo?.TestName ?? "NULL")}");
        }
        
        /// <summary>
        /// 广告展示成功
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // 广告显示
            // 2025-11-06 此点位改为由 paid 事件触发 -- By Yufei
            // SendAdImpEvent(adUnitId, adInfo);
        }

        private void SendAdImpEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var e = MaxAdEventBundleFactory.BuildIadsImp(adUnitId, adInfo, _adPlacement);
            _eventObserver?.OnEventIadsImp(e);
            Debug.Log($"{_tag} --- INTER Imp: {adUnitId}");
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
            var e = MaxAdEventBundleFactory.BuildIadsFailed(adUnitId, reason, errorInfo, _adStartLoadTime);
            _eventObserver?.OnEventIadsFailed(e);

            _isAdLoading = false;
            _ = MaxAdHelper.ReloadByRetryCount(_retryCount, ReloadAd);
            Debug.Log($"{_tag} --- INTER load Failed: {_maxAdUnitId } Info:{ errorInfo?.AdLoadFailureInfo ?? "Null" }");
        }
        
        private void OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // 广告显示失败
            var reason = "imp";
            var e = MaxAdEventBundleFactory.BuildIadsFailed(adUnitId, reason, errorInfo, _adStartDisplayTime);
            _eventObserver?.OnEventIadsFailed(e);
            _ = MaxAdHelper.ReloadByRetryCount(_retryCount, ReloadAd);
            Debug.Log($"{_tag} --- INTER display Failed: { _maxAdUnitId } Info:{ errorInfo?.AdLoadFailureInfo ?? "Null" }");
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
        private void OnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // 广告点击
            var e = MaxAdEventBundleFactory.BuildIadsClick(adUnitId, adInfo, _adPlacement);
            _eventObserver?.OnEventIadsClick(e);
            Debug.Log($"{_tag} --- INTER Click: { _maxAdUnitId}");
        }
        
        /// <summary>
        /// 广告收益
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // 广告收益
            var e = MaxAdEventBundleFactory.BuildIadsPaid(adUnitId, adInfo, _adPlacement);
            _eventObserver?.OnEventIadsPaid(e);
            Debug.Log($"{_tag} --- INTER Paid:: Revenue:{ adInfo.Revenue}");

            // 2025-11-06 由于 MAX 自身的主线程回归问题导致 IMP 时间延迟到 ADClose 之后才会调用，因此根据聪哥的建议，在 Paid 事件内触发 IMP 事件
            // 详见：https://www.tapd.cn/33527076/prong/stories/view/1133527076001022606?from_iteration_id=1133527076001002778
            SendAdImpEvent(adUnitId, adInfo);
        }
        
        /// <summary>
        /// 广告获取 reviewCreativeId
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="reviewCreativeId"></param>
        /// <param name="adInfo"></param>
        private void OnAdsReviewCreativeIdGeneratedEvent(string adUnitId, string reviewCreativeId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log($"{_tag} --- INTER Get Rcid:{reviewCreativeId}");
        }
        
        private void OnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // 广告关闭
            var e = MaxAdEventBundleFactory.BuildIadsClose(_maxAdUnitId, _adPlacement, _adStartDisplayTime);
            _eventObserver?.OnEventIadsClose(e);
            Debug.Log($"{_tag} --- INTER Close:{_adPlacement}");

            if (_isAdLoading) _isAdLoading = false;
            // 关闭后自动拉起下一条广告
            Load();
        }
        

    }
}