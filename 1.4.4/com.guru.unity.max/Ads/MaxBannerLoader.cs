


namespace Guru.Ads.Max
{
    using System;
    using UnityEngine;
    using Guru.Ads;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    
    /// <summary>
    /// Max Banner 广告代理
    /// </summary>
    public class MaxBannerLoader
    {
        
        private const int BANNER_RELOAD_SECONDS = 30; // 自动重试加载间隔
        
        private readonly Color _backColor;
        private readonly float _width;
        private readonly ICustomAmazonLoader _customAmazonLoader;
        private readonly IAdEventObserver _eventObserver; // 广告事件监听器
        private readonly MaxReviewCreativeIdCache _reviewCreativeIdCache;
        private readonly bool _adaptiveBannerEnabled = false;
        
        private string _maxAdUnitId;
        private bool _isBannerVisible; // Banner 是否可见
        private bool _autoRefresh;
        private bool _hasBannerCreated = false; // Banner 是否被创建
        private int _loadedTimes;
        private int _failedTimes;
        private DateTime _adStartLoadTime;
        private string _adPlacement;
        private readonly string _tag;
        private bool _shouldReportImpEvent;
        private CancellationTokenSource _retryLoadCts;

        private bool _isLoading = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="width"></param>
        /// <param name="colorHexStr"></param>
        /// <param name="customAmazonLoader"></param>
        /// <param name="observer"></param>
        /// <param name="adUnitId"></param>
        /// <param name="enableAdaptiveBanner"></param>
        public MaxBannerLoader(string adUnitId, float width, string colorHexStr, ICustomAmazonLoader customAmazonLoader, IAdEventObserver observer, bool enableAdaptiveBanner = false)
        {
            _hasBannerCreated = false;
            _maxAdUnitId = adUnitId;
            _backColor = MaxAdHelper.HexToColor(colorHexStr);
            _width = width;
            _customAmazonLoader = customAmazonLoader;
            _eventObserver = observer;
            _tag = AdConst.LOG_TAG_MAX;
            _isLoading = false;
            _reviewCreativeIdCache = new MaxReviewCreativeIdCache(12);
            _adaptiveBannerEnabled = enableAdaptiveBanner;
            
            // --- Add Callbacks ---
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnAdsLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdPaidEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnAdsCollapsedEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnAdsExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdReviewCreativeIdGeneratedEvent += OnAdsReviewCreativeIdGeneratedEvent;
        }


        public bool IsBannerVisible
        {
            get => _isBannerVisible;
            set
            {
                _isBannerVisible = value;
                SetAutoRefresh(value);
            }
        }

        /// <summary>
        /// 设置已购买去广告
        /// </summary>
        public void SetBuyNoAds()
        {
            // 取消加载重试
            Hide();
        }

        public Rect GetBannerLayout()
        {
            return MaxSdk.GetBannerLayout(_maxAdUnitId);
        }
        
        /// <summary>
        /// 设置 BANNER AdUnitID
        /// </summary>
        /// <param name="adUnitId"></param>
        public void SetAdUnitId(string adUnitId) => _maxAdUnitId = adUnitId;

        public void SetAutoRefresh(bool value)
        {
            _autoRefresh = value;
            if (value)
            {
                MaxSdk.StartBannerAutoRefresh(_maxAdUnitId); // 开启 Banner 的自动刷新
            }
            else
            {
                MaxSdk.StopBannerAutoRefresh(_maxAdUnitId);
            }
        }
        
        public bool Enabled
        {
            set
            {
                if (value)
                {
                    Load();
                }
                else
                {
                    Disable();
                }
            }
        }

        private void CreateBannerIfNotExists()
        {
            if(_hasBannerCreated) return;
            _customAmazonLoader.RequestBanner(CreateMaxBanner);
        }

        private void CreateMaxBanner()
        {
            MaxSdk.CreateBanner(_maxAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerExtraParameter(_maxAdUnitId, "adaptive_banner", _adaptiveBannerEnabled? "true" : "false");
            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(_maxAdUnitId, _backColor);
            // MaxSdk.StartBannerAutoRefresh(_maxAdUnitId);
            if (_width > 0)
            {
                // 可由外部设置 Banner 宽度
                MaxSdk.SetBannerWidth(_maxAdUnitId, _width);
            }
            _hasBannerCreated = true;
            Debug.Log($"{_tag} --- BADS created: {_maxAdUnitId}   isAdaptiveBanner: {_adaptiveBannerEnabled}");
        }


        /// <summary>
        /// 加载 Banner
        /// </summary>
        public void Load()
        {
            CreateBannerIfNotExists();
            _adStartLoadTime = DateTime.UtcNow;

            // 加载广告
            MaxSdk.LoadBanner(_maxAdUnitId);
            _isLoading = true;
            
            // 广告加载
            var e = MaxAdEventBundleFactory.BuildBadsLoad(_maxAdUnitId, _adPlacement);
            _eventObserver.OnEventBadsLoad(e);
            Debug.Log($"{_tag} --- BADS Load: {_maxAdUnitId}");
        }

        /// <summary>
        /// 显示 Banner
        /// </summary>
        /// <param name="placement"></param>
        public async UniTask Show(string placement = "")
        {
            _adPlacement = placement;

            if (IsBannerVisible) return;
            
            // 数据清零
            _loadedTimes = 0;
            _failedTimes = 0;
            _shouldReportImpEvent = true;

            if (!_isLoading)
            {
                Load(); // 如果没有开始加载， 则会调用加载
            }
            
            // 由于在 Amazon 预加载的时候 Banner 可能还没有创建出来
            while (!_hasBannerCreated)
            {
                // 若此时 Amazon 广告还在加载中， 则需要等待
                await UniTask.Delay(TimeSpan.FromSeconds(1)); 
            }
            
            // 显示广告
            MaxSdk.ShowBanner(_maxAdUnitId);
            MaxSdk.SetBannerPlacement(_maxAdUnitId, _adPlacement);
            SetAutoRefresh(true); // 开启 Banner 的自动刷新
            IsBannerVisible = true;
            
            Debug.Log($"{_tag} --- BADS Show: {_maxAdUnitId}");
        }
        
        private void ReportBadsImpEvent()
        {
            var e = MaxAdEventBundleFactory.BuildBadsImp(_maxAdUnitId, _adPlacement);
            _eventObserver.OnEventBadsImp(e);
        }
        
        /// <summary>
        /// 隐藏 Banner
        /// </summary>
        public void Hide()
        {
            // 停止广告刷新
            SetAutoRefresh(false);
            MaxSdk.HideBanner(_maxAdUnitId); // 关闭 Banner 的自动刷新

            if (!IsBannerVisible) return;
            
            CancelRetryLoadCts(); // 取消重试加载
            
            IsBannerVisible = false;
            // 广告隐藏
            var e = MaxAdEventBundleFactory.BuildBadsHide(_loadedTimes, _failedTimes);
            _eventObserver.OnEventBadsHide(e);
            Debug.Log($"{_tag} --- BADS Hide: {_maxAdUnitId}");
        }
        

        /// <summary>
        /// Banner 加载成功
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdsLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // 加载成功
            var e = MaxAdEventBundleFactory.BuildBadsLoaded(adUnitId, _adPlacement, _adStartLoadTime, adInfo);
            _eventObserver.OnEventBadsLoaded(e);
            // 数据更新
            _loadedTimes++;
            _isLoading = false;
            
            _adStartLoadTime = DateTime.UtcNow;
            Debug.Log($"{_tag} --- BADS loaded {adUnitId} -> WaterfallName: {(adInfo.WaterfallInfo?.Name ?? "NULL")}  TestName: {(adInfo.WaterfallInfo?.TestName ?? "NULL")}");

            // 如果加载成功后，发现 Banner 未展示，则尝试展示
            if (_shouldReportImpEvent)
            {
                _shouldReportImpEvent = false;
                ReportBadsImpEvent();
            }

            // 取消正在进行的重试加载任务
            CancelRetryLoadCts();
        }
        
        /// <summary>
        /// Banner 加载失败
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="errorInfo"></param>
        private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _failedTimes++;
            _isLoading = false;
            // 加载失败
            var e= MaxAdEventBundleFactory.BuildBadsFailed(adUnitId, _adPlacement, errorInfo, _adStartLoadTime);
            _eventObserver.OnEventBadsFailed(e);
            // 刷新时间
            _adStartLoadTime = DateTime.UtcNow;
            
            Debug.Log($"{_tag} --- BADS load failed -> Failed count:{_failedTimes}  Info:{errorInfo.AdLoadFailureInfo}  autoRefresh:{_autoRefresh}");
            ReloadBannerAsync();
        }


        /// <summary>
        /// 异步重新加载 Banner
        /// </summary>
        private async void ReloadBannerAsync()
        {
            // 如果 Banner 没有显示则不做处理
            if (!IsBannerVisible) return;
            Debug.Log($"{_tag} --- BADS ReloadBannerAsync: {_maxAdUnitId}");
            
            try 
            {
                _retryLoadCts = new CancellationTokenSource();
                int delaySeconds = 0;
                while (IsBannerVisible)
                {
                    
                    // 没有网络
                    while (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        delaySeconds += AdConst.NO_NETWORK_WAITING_SECONDS;
                        // Debug.Log($"{_tag} --- Bads try Reload but no network: {Application.internetReachability}");
                        await UniTask.Delay(TimeSpan.FromSeconds(AdConst.NO_NETWORK_WAITING_SECONDS));
                    }

                    var waitingSeconds = Mathf.Max(1, (BANNER_RELOAD_SECONDS - delaySeconds));
                    // 等待自动重试加载
                    await UniTask.Delay(TimeSpan.FromSeconds(waitingSeconds), cancellationToken: _retryLoadCts.Token);

                    Disable();
                    await UniTask.DelayFrame(1);
                    Show().Forget();
                    
                    Debug.Log($"{_tag} --- BADS LoadFailHandler immediate with id: {_maxAdUnitId}");
                    break;
                }
                // Debug.Log($"{_tag} --- BADS ReloadBannerAsync over");
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Debug.Log($"{_tag} --- BADS LoadFailHandler cancelled: {_maxAdUnitId}");
                }
                else
                {
                    Debug.LogError($"{_tag} --- BADS LoadFailHandler with Error: {ex.Message}");
                }
            }
            finally
            {
                // Debug.Log($"{_tag} --- BADS Reload Dispose: {_retryLoadCts.Token}");
                _retryLoadCts?.Dispose();
                _retryLoadCts = null;
            }
        }



        private void CancelRetryLoadCts()
        {
            if (_retryLoadCts == null || _retryLoadCts.IsCancellationRequested) return;
            Debug.Log($"{_tag} --- BADS CancelRetryLoadCts");
            _retryLoadCts.Cancel();
            _retryLoadCts.Dispose();
            _retryLoadCts = null;
        }


        private void OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // No Callback so no Implemention
        }
        
        /// <summary>
        /// Banner 被点击
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // 广告点击
            var e = MaxAdEventBundleFactory.BuildBadsClick(adUnitId, adInfo.Placement);
            _eventObserver.OnEventBadsClick(e);
            
            Debug.Log($"{_tag} --- BADS Click: {_maxAdUnitId}");
        }
        
        /// <summary>
        /// Banner 收益
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (!IsBannerVisible) return; // Banner 隐藏时不上报广告事件
            var reviewedCreativeId = _reviewCreativeIdCache.GetReviewedCreativeId(adInfo);
            // 广告收益
            var e = MaxAdEventBundleFactory.BuildBadsPaid(adUnitId, adInfo, _adPlacement, reviewedCreativeId);
            _eventObserver.OnEventBadsPaid(e);
        }



        /// <summary>
        /// Banner 获取 reviewCreativeId
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="reviewCreativeId"></param>
        /// <param name="adInfo"></param>
        private void OnAdsReviewCreativeIdGeneratedEvent(string adUnitId, string reviewCreativeId, MaxSdkBase.AdInfo adInfo)
        {
            UnityEngine.Debug.Log($"{_tag} --- BADS get rcid::{reviewCreativeId}");
            _reviewCreativeIdCache.AddOrUpdate(adInfo, reviewCreativeId);
        }

        private void OnAdsCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            UnityEngine.Debug.Log($"{_tag} --- BADS Collapsed: {adUnitId}");
        }
        
        private void OnAdsExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            UnityEngine.Debug.Log($"{_tag} --- BADS Expanded: {adUnitId}");
        }

       

        /// <summary>
        /// 消除 Banner
        /// 去广告后请调用消除 Banner
        /// </summary>
        /// <param name="adUnitId"></param>
        private void Disable()
        {
            Hide();
            DestroyMaxBanner();
        }
        
        private void DestroyMaxBanner()
        {
            MaxSdk.DestroyBanner(_maxAdUnitId);
            _hasBannerCreated = false;
        }


    }
}