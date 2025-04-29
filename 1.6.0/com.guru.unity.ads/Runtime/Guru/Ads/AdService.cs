

namespace Guru.Ads
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    
    public class AdService: IAdEventObserver
    {
        public const string LOG_TAG = "[Ads]";


        private static readonly List<string> MainThreadEvents = new List<string>()
        {
            // Banner
            nameof(OnEventBadsLoad),
            nameof(OnEventBadsLoaded),
            nameof(OnEventBadsFailed),
            nameof(OnEventBadsPaid),
            
            // IADS
            nameof(OnEventIadsImp),
            nameof(OnEventIadsLoaded),
            nameof(OnEventIadsFailed),
            nameof(OnEventIadsClick),
            nameof(OnEventIadsClose),
            
            // RAADS
            nameof(OnEventRadsImp),
            nameof(OnEventRadsLoaded),
            nameof(OnEventRadsFailed),
            nameof(OnEventRadsClose),
            nameof(OnEventRadsClick),
            nameof(OnEventRadsRewarded),
        };
        
        
        // 单例
        private static AdService _instance;
        public static AdService Instance
        {
            get
            {
                if (_instance == null) _instance = new AdService();
                return _instance;
            }
        }

        public string GetMediationLogTag() => Instance._adManager?.GetLogTag() ?? string.Empty;
        
        private AdsModel _model;
        public AdsModel Model
        {
            get
            {
                if (_model == null) _model = AdsModel.Create();     
                return _model;
            }
        }
        
        private double Tch001Value
        {
            get => Model.TchAD001RevValue;
            set => Model.TchAD001RevValue = value;
        }

        private double Tch02Value
        {
            get => Model.TchAD02RevValue;
            set => Model.TchAD02RevValue = value;
        }

        private HashSet<string> _blockedEvents;
        private IAdManager _adManager;
        private bool? _invokeEventOnMainThread;
        
        private bool _hasGetRvRewarded = false;
        
        #region 对外回调接口定义
        
        // ----------------- 广告事件的回调接口 ----------------- 
        // BANNER
        private Action<BadsLoadEvent> _onBadsLoad;
        private Action<BadsLoadedEvent> _onBadsLoaded;
        private Action<BadsFailedEvent> _onBadsFailed;
        private Action<BadsImpEvent> _onBadsImp;
        private Action<BadsHideEvent> _onBadsHide;
        private Action<BadsClickEvent> _onBadsClick;
        private Action<BadsPaidEvent> _onBadsPaid;
        // INTER
        private Action<IadsLoadEvent> _onIadsLoad;
        private Action<IadsLoadedEvent> _onIadsLoaded;
        private Action<IadsFailedEvent> _onIadsFailed;
        private Action<IadsImpEvent> _onIadsImp;
        private Action<IadsCloseEvent> _onIadsClose;
        private Action<IadsClickEvent> _onIadsClick;
        private Action<IadsPaidEvent> _onIadsPaid;
        // REWARDED
        private Action<RadsLoadEvent> _onRadsLoad;
        private Action<RadsLoadedEvent> _onRadsLoaded;
        private Action<RadsFailedEvent> _onRadsFailed;
        private Action<RadsImpEvent> _onRadsImp;
        private Action<RadsCloseEvent> _onRadsClose;
        private Action<RadsClickEvent> _onRadsClick;
        private Action<RadsPaidEvent> _onRadsPaid;
        private Action<RadsRewardedEvent> _onRadsRewarded;
        #endregion

        #region 客户端回调接口

        private Action _customIadsCloseHandler;
        
        [Obsolete("已废弃")]
        private Action _customRadsRewardedHandler;
        [Obsolete("已废弃")]
        private Action _customRadsCloseHandler;
        
        private Action<bool, string> _customRadsResultHandler;
        
        #endregion

        #region 初始化函数

        /// <summary>
        /// 构造函数
        /// </summary>
        private AdService()
        {
            _model = AdsModel.Create();
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="adManager"></param>
        /// <param name="blockedEvents">屏蔽事件名单</param>
        /// <param name="invokeEventOnMainThread">广告事件依赖主线程触发</param>
        public void StartService(IAdManager adManager, HashSet<string> blockedEvents = null, bool? invokeEventOnMainThread = null)
        {
            // 设置 AdManger
            _adManager = adManager;
            // 屏蔽事件清单
            _blockedEvents = blockedEvents;
            _invokeEventOnMainThread = invokeEventOnMainThread;
        }
        
        #endregion
        
        #region 事件上报
        
        /// <summary>
        /// 上报广告打点
        /// </summary>
        /// <param name="adEvent"></param>
        private void ReportAdEvent(ITrackingEvent adEvent)
        {
            if (IsBlocked(adEvent.EventName))
            {
                Debug.LogWarning($"{LOG_TAG} --- event is blocked: {adEvent.EventName}");
                return;
            }
            
            Analytics.TrackEvent(adEvent);
        }

        private bool IsBlocked(string eventName)
        {
            if (_blockedEvents == null) return false;
            return _blockedEvents.Contains(eventName);
        }

        #endregion
        
        #region Revenue 统计

        /// <summary>
        /// 广告收入数据
        /// </summary>
        /// <param name="paidEvent"></param>
        private void ReportAdPaidEvent(AbstractAdPaidEvent paidEvent)
        {
            // 上报 paid 事件
            ReportAdEvent(paidEvent);
            
            // 上报 ad_impression 事件
            var impressionEvent = paidEvent.ToAdImpressionEvent();
            ReportAdEvent(impressionEvent);

            // 上报 tch 事件
            var revenue = paidEvent.value;
            AddAdsTch001Revenue(revenue);
            AddAdsTch02Revenue(revenue);
            
            // 筛选 FB Network
            TrySaveFBAdRevenueDate(paidEvent.adSource);
        }

        /// <summary>
        /// 尝试保存 FB 收益日期
        /// </summary>
        /// <param name="adSource"></param>
        private void TrySaveFBAdRevenueDate(string adSource)
        {
            if (adSource.ToLower().Contains("facebook"))
            {
                Model.PreviousFBAdRevenueDate = DateTime.UtcNow; // 记录当前 FB 的收益日期
            }
        }

        /// <summary>
        /// 累积计算太极001收益
        /// </summary>
        /// <param name="revenue"></param>
        private void AddAdsTch001Revenue(double revenue)
        {
            Tch001Value += revenue;
            double revenueValue = Tch001Value;
            Debug.Log($"{LOG_TAG} --- [Tch] <tch_ad_rev_roas_001> with value: {revenueValue}");
            if (revenueValue < Analytics.TCH_001_VALUE) return;
            Analytics.Tch001ADRev(revenueValue, _adManager.GetMediationName());
            Tch001Value = 0.0;
        }

        /// <summary>
        /// 累积计算太极02收益
        /// </summary>
        /// <param name="revenue"></param>
        private void AddAdsTch02Revenue(double revenue)
        {
            Tch02Value += revenue;
            double revenueValue = Tch02Value;
            Debug.Log($"{LOG_TAG} --- [Tch] <tch_ad_rev_roas_02> with value: {revenueValue}");
            if (revenueValue < Analytics.TCH_02_VALUE) return;
            Analytics.Tch02ADRev(revenueValue, _adManager.GetMediationName());
            Tch02Value = 0.0;
        }

        #endregion

        #region IAdEventObserver 接口实现

        
        // ------ BANNER ------
        public void OnEventBadsLoad(BadsLoadEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventBadsLoad, evt);
        }
        private void ForwardEventBadsLoad(BadsLoadEvent evt)
        {
            _onBadsLoad?.Invoke(evt);
        }
        
        public void OnEventBadsLoaded(BadsLoadedEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventBadsLoaded, evt);
        }
        private void ForwardEventBadsLoaded(BadsLoadedEvent evt)
        {
            _onBadsLoaded?.Invoke(evt);
        }
        
        public void OnEventBadsFailed(BadsFailedEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventBadsFailed, evt);
        }
        private void ForwardEventBadsFailed(BadsFailedEvent evt)
        {
            _onBadsFailed?.Invoke(evt);
        }

        public void OnEventBadsImp(BadsImpEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventBadsImp, evt);
        }

        private void ForwardEventBadsImp(BadsImpEvent evt)
        {
            _onBadsImp?.Invoke(evt);
        }

        public void OnEventBadsHide(BadsHideEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventBadsHide, evt);
        }

        private void ForwardEventBadsHide(BadsHideEvent evt)
        {
            _onBadsHide?.Invoke(evt);
        }

        public void OnEventBadsClick(BadsClickEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventBadsClick, evt);
        }
        private void ForwardEventBadsClick(BadsClickEvent evt)
        {
            _onBadsClick?.Invoke(evt); 
        }

        public void OnEventBadsPaid(BadsPaidEvent evt)
        {
            // ad_impression
            ReportAdPaidEvent(evt);
            PostToMainThread(ForwardEventBadsPaid, evt);
        }
        private void ForwardEventBadsPaid(BadsPaidEvent evt)
        {
            _onBadsPaid?.Invoke(evt);
        }


        // ------ INTER ------
        
        public void OnEventIadsLoad(IadsLoadEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventIadsLoad, evt);
        }
        private void ForwardEventIadsLoad(IadsLoadEvent evt)
        {
            _onIadsLoad?.Invoke(evt);
        }

        public void OnEventIadsLoaded(IadsLoadedEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventIadsLoaded, evt);
        }

        private void ForwardEventIadsLoaded(IadsLoadedEvent evt)
        {
            _onIadsLoaded?.Invoke(evt);
        }

        public void OnEventIadsFailed(IadsFailedEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventIadsFailed, evt);
        }

        private void ForwardEventIadsFailed(IadsFailedEvent evt)
        {
            _onIadsFailed?.Invoke(evt);
        }

        public void OnEventIadsImp(IadsImpEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventIadsImp, evt);
        }
        private void ForwardEventIadsImp(IadsImpEvent evt)
        {
            _onIadsImp?.Invoke(evt);
        }

        public void OnEventIadsClick(IadsClickEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventIadsClick, evt);
        }
        private void ForwardEventIadsClick(IadsClickEvent evt)
        {
            _onIadsClick?.Invoke(evt);
        }

        public void OnEventIadsClose(IadsCloseEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventIadsClose, evt);
        }
        private void ForwardEventIadsClose(IadsCloseEvent evt)
        {
            _onIadsClose?.Invoke(evt);
            _customIadsCloseHandler?.Invoke();
            _customIadsCloseHandler = null;
        }
        
        public void OnEventIadsPaid(IadsPaidEvent evt)
        {
            // ad_impression
            ReportAdPaidEvent(evt);
            PostToMainThread(ForwardEventIadsPaid, evt);
        }

        private void ForwardEventIadsPaid(IadsPaidEvent evt)
        {
            _onIadsPaid?.Invoke(evt);
        }

        // REWARDED
        public void OnEventRadsLoad(RadsLoadEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventRadsLoad, evt);
        }
        private void ForwardEventRadsLoad(RadsLoadEvent evt)
        {
            _onRadsLoad?.Invoke(evt);
        }

        public void OnEventRadsLoaded(RadsLoadedEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventRadsLoaded, evt);
        }
        private void ForwardEventRadsLoaded(RadsLoadedEvent evt)
        {
            _onRadsLoaded?.Invoke(evt);
        }

        public void OnEventRadsFailed(RadsFailedEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventRadsFailed, evt);
        }

        private void ForwardEventRadsFailed(RadsFailedEvent evt)
        {
            _onRadsFailed?.Invoke(evt);
        }

        public void OnEventRadsImp(RadsImpEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventRadsImp, evt);
        }

        private void ForwardEventRadsImp(RadsImpEvent evt)
        {
            _onRadsImp?.Invoke(evt);
        }

        public void OnEventRadsClick(RadsClickEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventRadsClick, evt);
        }
        private void ForwardEventRadsClick(RadsClickEvent evt)
        {
            _onRadsClick?.Invoke(evt);
        }


        public void OnEventRadsClose(RadsCloseEvent evt)
        {
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventRadsCloseEvent, evt);
        }
        private void ForwardEventRadsCloseEvent(RadsCloseEvent evt)
        {
            _onRadsClose?.Invoke(evt);
            // _customRadsCloseHandler?.Invoke();
            _customRadsResultHandler.Invoke(_hasGetRvRewarded, evt.placement);
            _customRadsResultHandler = null;
        }

        public void OnEventRadsRewarded(RadsRewardedEvent evt)
        {
            _hasGetRvRewarded = true;
            ReportAdEvent(evt);
            PostToMainThread(ForwardEventRadsRewarded, evt);
        }
        private void ForwardEventRadsRewarded(RadsRewardedEvent evt)
        {
            _onRadsRewarded?.Invoke(evt); // 外部回调方法
            // _customRadsRewardedHandler?.Invoke();
            
            // 校验是否是首次 Rads 奖励
            if (Model.HasFirstRadsReward) return;
            Model.HasFirstRadsReward = true;
            OnEventFirstRadsRewarded(evt.adUnitId, evt.placement);
        }
        

        private void OnEventFirstRadsRewarded(string adUnitId, string placement)
        {
            var firstRadsRewarded = new RadsFirstRewardedEvent(adUnitId, placement);
            ReportAdEvent(firstRadsRewarded);
        }
        

        public void OnEventRadsPaid(RadsPaidEvent evt)
        {
            // Impression Data
            ReportAdPaidEvent(evt);
            PostToMainThread(ForwardEventRadsPaid, evt);
        }
        private void ForwardEventRadsPaid(RadsPaidEvent evt)
        {
            _onRadsPaid?.Invoke(evt);
        }

        /// <summary>
        /// 为了保证时序，想主线程发送事件
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="evt"></param>
        /// <typeparam name="T"></typeparam>
        private void PostToMainThread<T>(Action<T> callback, T evt)
        {
            if (callback == null) return;
            
            UniTask.Post(() =>
            {
                callback.Invoke(evt);
            });
        }



        #endregion

        #region 广告接口

        public bool IsBuyNoAds
        {
            get => Model.BuyNoAds;
            set => Model.BuyNoAds = value;
        }


        /// <summary>
        /// 插屏广告是否就绪
        /// </summary>
        /// <returns></returns>
        public bool IsInterstitialReady()
        {
            return _adManager?.IsInterstitialReady() ?? false;
        }

        /// <summary>
        /// 激励视频是否就绪
        /// </summary>
        /// <returns></returns>
        public bool IsRewardedReady()
        {
            return _adManager?.IsRewardedReady() ?? false;
        }
        
        // ------------ Banner ---------------
        
        /// <summary>
        /// Banner是否显示
        /// </summary>
        /// <returns></returns>
        public bool IsBannerVisible()
        {
            return _adManager?.IsBannerVisible() ?? false;
        }
        
        /// <summary>
        /// 显示 Banner
        /// </summary>
        /// <param name="placement"></param>
        public void ShowBanner(string placement)
        {
            if (_adManager == null) return;
            if (!_adManager.IsReady()) return;
            _adManager.ShowBanner(placement);
        }

        /// <summary>
        /// 隐藏 Banner
        /// </summary>
        public void HideBanner()
        {
            if (_adManager == null) return;
            if (!_adManager.IsReady()) return;
            _adManager.HideBanner();
        }
        
        /// <summary>
        /// 获取 Banner 广告 Layout
        /// </summary>
        /// <returns></returns>
        public Rect GetBannerLayout()
        {
            if (!_adManager.IsReady()) 
                return new Rect();
            
            return _adManager.GetBannerLayout();
        }

        // ------------ Interstitial ---------------
        public void LoadInterstitial()
        {
            Debug.Log($"{LOG_TAG} Services: LoadInterstitialAd");
            _adManager.LoadInterstitial();
        }
        
        public void ShowInterstitial(string placement, Action onCloseHandler = null)
        {
            _customIadsCloseHandler = onCloseHandler;
            _adManager.ShowInterstitial(placement);
        }

        public void EnableNoAds()
        {
            if (Model.BuyNoAds) return;
            Model.BuyNoAds = true;
            _adManager?.EnableNoAds();
        }


        // ------------ Rewarded ---------------
        
        public void LoadRewarded()
        {
            Debug.Log($"{LOG_TAG} Services: LoadRewarded");
            _adManager.LoadRewarded();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="placement"></param>
        /// <param name="onRewardedResult"></param>
        public void ShowRewarded(string placement, Action<bool, string> onRewardedResult = null)
        {
            _hasGetRvRewarded = false;
            _customRadsResultHandler = onRewardedResult;
            _adManager.ShowRewarded(placement);
        }



        [Obsolete("此方法已经废弃， 请勿再调用! 可使用 ShowRewarded(string placement, Action<bool, string, string> onCloseHandler = null) 来执行回调")]
        public void ShowRewarded(string placement, Action onRewardedHandler = null, Action onCloseHandler = null)
        {

            throw new Exception("Can not call ShowRewarded function is Obsolete");
            
            _customRadsRewardedHandler = onRewardedHandler;
            _customRadsCloseHandler = onCloseHandler;
            _adManager.ShowRewarded(placement);
        }
        
        

        #endregion

        #region 生命周期

        /// <summary>
        /// 应用暂停时的响应逻辑
        /// </summary>
        /// <param name="isPaused"></param>
        public void SetAppPause(bool isPaused)
        {
            _adManager?.SetAppPause(isPaused);
        }

        public bool IsReady()
        {
            return _adManager?.IsReady() ?? false;
        }


        #endregion
        
        #region 回调接口挂载实现
        // BANNER
        public event Action<BadsLoadEvent> OnBadsLoad
        {
            add => _onBadsLoad += value;
            remove => _onBadsLoad -= value;
        }

        public event Action<BadsLoadedEvent> OnBadsLoaded
        {
            add => _onBadsLoaded += value;
            remove => _onBadsLoaded -= value;
        }

        public event Action<BadsFailedEvent> OnBadsFailed
        {
            add => _onBadsFailed += value;
            remove => _onBadsFailed -= value;
        }

        public event Action<BadsImpEvent> OnBadsImp
        {
            add => _onBadsImp += value;
            remove => _onBadsImp -= value;
        }   

        public event Action<BadsHideEvent> OnBadsHide
        {
            add => _onBadsHide += value;
            remove => _onBadsHide -= value;
        }

        public event Action<BadsClickEvent> OnBadsClick
        {
            add => _onBadsClick += value;
            remove => _onBadsClick -= value;
        }

        public event Action<BadsPaidEvent> OnBadsPaid
        {
            add => _onBadsPaid += value;
            remove => _onBadsPaid -= value;
        }
        
        // INTER
        public event Action<IadsLoadEvent> OnIadsLoad
        {
            add => _onIadsLoad += value;
            remove => _onIadsLoad -= value;
        }

        public event Action<IadsLoadedEvent> OnIadsLoaded
        {
            add => _onIadsLoaded += value;
            remove => _onIadsLoaded -= value;
        }   

        public event Action<IadsFailedEvent> OnIadsFailed
        {
            add => _onIadsFailed += value;
            remove => _onIadsFailed -= value;
        }
        
        public event Action<IadsImpEvent> OnIadsImp
        {
            add => _onIadsImp += value;
            remove => _onIadsImp -= value;
        }
        
        public event Action<IadsCloseEvent> OnIadsClose
        {
            add => _onIadsClose += value;
            remove => _onIadsClose -= value;
        }   

        public event Action<IadsClickEvent> OnIadsClick
        {
            add => _onIadsClick += value;
            remove => _onIadsClick -= value;
        }

        public event Action<IadsPaidEvent> OnIadsPaid
        {
            add => _onIadsPaid += value;
            remove => _onIadsPaid -= value;
        }

        // REWARDED
        
        public event Action<RadsLoadEvent> OnRadsLoad
        {
            add => _onRadsLoad += value;
            remove => _onRadsLoad -= value;
        }
        
        
        public event Action<RadsLoadedEvent> OnRadsLoaded
        {
            add => _onRadsLoaded += value;
            remove => _onRadsLoaded -= value;
        }
        
        public event Action<RadsFailedEvent> OnRadsFailed
        {
            add => _onRadsFailed += value;
            remove => _onRadsFailed -= value;
        }
        
        public event Action<RadsImpEvent> OnRadsImp
        {
            add => _onRadsImp += value;
            remove => _onRadsImp -= value;
        }              

        public event Action<RadsCloseEvent> OnRadsClose
        {
            add => _onRadsClose += value;
            remove => _onRadsClose -= value;
        }

        public event Action<RadsClickEvent> OnRadsClick
        {
            add => _onRadsClick += value;
            remove => _onRadsClick -= value;
        }

        public event Action<RadsPaidEvent> OnRadsPaid
        {
            add => _onRadsPaid += value;
            remove => _onRadsPaid -= value;
        }
        
        public event Action<RadsRewardedEvent> OnRadsRewarded
        {
            add => _onRadsRewarded += value;
            remove => _onRadsRewarded -= value;
        }

        #endregion
        
        #region 调式接口
        
        /// <summary>
        /// 显示 Mediation Debugger
        /// </summary>
        public void ShowMaxMediationDebugger()
        {
            if (_adManager is IMaxDebugger maxDebugger)
            {
                maxDebugger.ShowMediationDebugger();
            }
        }
        
        /// <summary>
        /// 显示 Creative Debugger
        /// </summary>
        public void ShowMaxCreativeDebugger()
        {
            if (_adManager is IMaxDebugger maxDebugger)
            {
                maxDebugger.ShowCreativeDebugger();
            }
        }
        

        /// <summary>
        /// Debug 设置 tch02 的收入参数
        /// 可立即触发 tch_02 事件
        /// </summary>
        /// <param name="value"></param>
        public void SetTch001Revenue(double value = 0.01)
        {
            AddAdsTch001Revenue(value);
        }
        
        /// <summary>
        /// Debug 设置 tch02 的收入参数
        /// 可立即触发 tch_02 事件
        /// </summary>
        /// <param name="value"></param>
        public void SetTch02Revenue(double value = 0.2)
        {
            AddAdsTch02Revenue(value);
        }
        
        #endregion

        #region 自定义广告参数接口

        public void SetBannerAdUnitId(string adUnitId)
        {
            _adManager.SetBannerAdUnitId(adUnitId);
        }
        
        public void SetInterstitialAdUnitId(string adUnitId)
        {
            _adManager.SetInterstitialAdUnitId(adUnitId);
        }
        
        public void SetRewardedAdUnitId(string adUnitId)
        {
            _adManager.SetRewardedAdUnitId(adUnitId);
        }


        #endregion

        
        
    }
    
    
    
    
    
}