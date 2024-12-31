
namespace Guru.Ads.Max
{
    using System;
    using UnityEngine;
    using Guru.Ads;
    using System.Collections.Generic;
    
    /// <summary>
    /// Max Banner 广告代理
    /// </summary>
    public class MaxBannerLoader
    {
        private readonly Color _backColor;
        private readonly float _width;
        private readonly MaxAmazonPreLoader _amazonPreLoader;
        private readonly IAdEventObserver _eventObserver; // 广告事件监听器

        private string _maxAdUnitId;
        private bool _isBannerVisible; // Banner 是否可见
        private bool _hasBannerCreated = false; // Banner 是否被创建
        private int _loadedTimes;
        private int _failedTimes;
        private DateTime _adStartLoadTime;
        private string _adPlacement;
        private readonly string _tag;
        private bool _shouldReportImpEvent;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="width"></param>
        /// <param name="colorHexStr"></param>
        /// <param name="amazonPreLoader"></param>
        /// <param name="observer"></param>
        /// <param name="adUnitId"></param>
        public MaxBannerLoader(string adUnitId, float width, string colorHexStr, MaxAmazonPreLoader amazonPreLoader, IAdEventObserver observer)
        {
            _hasBannerCreated = false;
            _maxAdUnitId = adUnitId;
            _backColor = HexToColor(colorHexStr);
            _width = width;
            _amazonPreLoader = amazonPreLoader;
            _eventObserver = observer;
            _tag = AdConst.LOG_TAG_MAX;
            
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
            
            _amazonPreLoader.PreLoadBanner();
            
            MaxSdk.CreateBanner(_maxAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerExtraParameter(_maxAdUnitId, "adaptive_banner", "false");
            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(_maxAdUnitId, _backColor);
            // MaxSdk.StartBannerAutoRefresh(_maxAdUnitId);
            if (_width > 0)
            {
                // 可由外部设置 Banner 宽度
                MaxSdk.SetBannerWidth(_maxAdUnitId, _width);
            }
            _hasBannerCreated = true;
            Debug.Log($"{_tag} --- BADS created: {_maxAdUnitId}");
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
            
            // 广告加载
            var e = MaxAdEventBundleFactory.BuildBadsLoad(_maxAdUnitId, _adPlacement);
            _eventObserver.OnEventBadsLoad(e);
            Debug.Log($"{_tag} --- BADS Load: {_maxAdUnitId}");
        }

        /// <summary>
        /// 显示 Banner
        /// </summary>
        /// <param name="placement"></param>
        public void Show(string placement = "")
        {
            _adPlacement = placement;

            if (IsBannerVisible) return;


            // 显示广告
            MaxSdk.ShowBanner(_maxAdUnitId);
            MaxSdk.SetBannerPlacement(_maxAdUnitId, _adPlacement);
            SetAutoRefresh(true); // 开启 Banner 的自动刷新
            
            // 数据清零
            _loadedTimes = 0;
            _failedTimes = 0;
            _shouldReportImpEvent = true;
            IsBannerVisible = true;
            
                        
            // if (!_hasBannerCreated)
            // {
            // 被销毁后再调用
            Load();
            // }
            
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
            
            _adStartLoadTime = DateTime.UtcNow;
            Debug.Log($"{_tag} --- BADS loaded {adUnitId} -> WaterfallName: {(adInfo.WaterfallInfo?.Name ?? "NULL")}  TestName: {(adInfo.WaterfallInfo?.TestName ?? "NULL")}");

            // 如果加载成功后，发现 Banner 未展示，则尝试展示
            if (_shouldReportImpEvent)
            {
                _shouldReportImpEvent = false;
                ReportBadsImpEvent();
            }
        }
        
        /// <summary>
        /// Banner 加载失败
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="errorInfo"></param>
        private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _failedTimes++;
            // 加载失败
            var e= MaxAdEventBundleFactory.BuildBadsFailed(adUnitId, _adPlacement, errorInfo, _adStartLoadTime);
            _eventObserver.OnEventBadsFailed(e);
            // 刷新时间
            _adStartLoadTime = DateTime.UtcNow;
            
            Debug.LogError($"{_tag} --- BADS load failed: {_maxAdUnitId}  Info:{errorInfo.AdLoadFailureInfo}");
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
            // 广告收益
            var e = MaxAdEventBundleFactory.BuildBadsPaid(adUnitId, adInfo, _adPlacement);
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
        }

        private void OnAdsCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            UnityEngine.Debug.Log($"{_tag} --- BADS Collapsed: {adUnitId}");
        }
        
        private void OnAdsExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            UnityEngine.Debug.Log($"{_tag} --- BADS Expanded: {adUnitId}");
        }

        private Color HexToColor(string hexString)
        {
            if(string.IsNullOrEmpty(hexString)) return Color.clear;
            
            var hex = hexString.Replace("#", "");
            if(hex.Length < 6) return Color.clear;
            
            int num = System.Convert.ToInt32(hex, 16);
 
            // 将一个十六进制数转换为Color
            // 假设十六进制字符串是 RRGGBBAA 格式
            byte r = (byte)(num & 0xFF); // 红色
            byte g = (byte)(num >> 8 & 0xFF); // 绿色
            byte b = (byte)(num >> 16 & 0xFF); // 蓝色
            byte a = (byte)(num >> 24 & 0xFF); // 透明度
            // 创建Color对象
            Debug.Log($"{_tag} --- bgColor: ({r},{g},{b},{a})");
            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// 消除 Banner
        /// 去广告后请调用消除 Banner
        /// </summary>
        /// <param name="adUnitId"></param>
        private void Disable()
        {
            _isBannerVisible = false;
            MaxSdk.DestroyBanner(_maxAdUnitId);
            _hasBannerCreated = false;
        }


    }
}