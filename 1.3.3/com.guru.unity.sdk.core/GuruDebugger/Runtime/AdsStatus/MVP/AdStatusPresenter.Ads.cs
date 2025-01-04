
namespace Guru
{
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using Const=AdStatusConsts;
    using Guru.Ads;
    
    public partial class AdStatusPresenter
    {
        private Queue<AdStatusInfo> _bannerInfos = new Queue<AdStatusInfo>(AdStatusConsts.MaxInfoCount);
        private Queue<AdStatusInfo> _interInfos = new Queue<AdStatusInfo>(AdStatusConsts.MaxInfoCount);
        private Queue<AdStatusInfo> _rewardInfos = new Queue<AdStatusInfo>(AdStatusConsts.MaxInfoCount);

        private AdStatusInfo _curBadsInfo;
        private AdStatusInfo _curIadsInfo;
        private AdStatusInfo _curRadsInfo;
        
        #region InfoContainer

        /// <summary>
        /// 添加对应的 Info
        /// </summary>
        /// <param name="info"></param>
        private void AddBannerInfo(AdStatusInfo info)
        {
            if (info == null) return;
            
            if (_bannerInfos != null)
            {
                if (_bannerInfos.Count >= AdStatusConsts.MaxInfoCount)
                {
                    _bannerInfos.Dequeue();
                }
                _bannerInfos.Enqueue(info);
            }
            
            // Debug.Log(info.ToLogString());
            _curBadsInfo = info;
            OnStatueChanged(info);
        }
        
        private void AddInterInfo(AdStatusInfo info)
        {
            if (info == null) return;
            
            if (_interInfos != null)
            {
                if (_interInfos.Count >= AdStatusConsts.MaxInfoCount)
                {
                    _interInfos.Dequeue();
                }
                _interInfos.Enqueue(info);
            }
            
            _curIadsInfo = info;
            OnStatueChanged(info);
        }
        
        private void AddRewardInfo(AdStatusInfo info)
        {
            if (info == null) return;
            
            if (_rewardInfos != null)
            {
                if (_rewardInfos.Count >= AdStatusConsts.MaxInfoCount)
                {
                    _rewardInfos.Dequeue();
                }
                _rewardInfos.Enqueue(info);
            }
            
            _curRadsInfo = info;
            OnStatueChanged(info);
        }

        /// <summary>
        /// 状态刷新
        /// </summary>
        /// <param name="info"></param>
        private void OnStatueChanged(AdStatusInfo info)
        {
            if (_model == null) return;
            _model.monitorInfo = CreateMonitorInfo();
            
            if (info != null)
            {
                int code = 0;
                if (info.status == AdStatusType.Failed)
                {
                    code = -1;
                }
                else if (info.status == AdStatusType.Loaded)
                {
                    code = 1;
                }
                
                switch (info.adType)
                {
                    case AdType.Banner:
                        if(code == 1) _model.AddBannerCount(true);
                        else if(code == -1) _model.AddBannerCount(false);
                        break;
                    case AdType.Interstitial:
                        if(code == 1) _model.AddInterCount(true);
                        else if(code == -1) _model.AddInterCount(false);
                        break;
                    case AdType.Rewarded:
                        if(code == 1) _model.AddRewardCount(true);
                        else if(code == -1) _model.AddRewardCount(false);
                        break;
                }
            }

            UpdateView(); // 刷新视图
        }

        // 字段缓冲
        private StringBuilder _infoBuff;
        private string CreateMonitorInfo()
        {
            string msg;
            if ( !AdService.Instance.IsReady())
            {
                msg = ColoredText("AdService not initialized...", Const.ColorRed);
                return msg;
            }
            
            if (_infoBuff == null) _infoBuff = new StringBuilder();
            _infoBuff.Clear();
            
            if (_curBadsInfo == null)
            {
                msg = $"BADS: {ColoredText("not ready", Const.ColorRed)}\n";
            }
            else
            {

                switch (_curBadsInfo.status)
                {
                    case AdStatusType.Loaded:
                        msg = $"BADS: {ColoredText("loaded", Const.ColorGreen)}\n\tnetwork: {_curBadsInfo.network}\n\twaterfall: {_curBadsInfo.waterfall}\n";
                        break;
                    case AdStatusType.Failed:
                        msg = $"BADS: {ColoredText("failed", Const.ColorRed)}\n\terrorCode: {_curBadsInfo.errorCode}\n";
                        break;
                    case AdStatusType.Loading:
                        msg = $"BADS: {ColoredText("loading...", Const.ColorYellow)}\n\tadId: {_curBadsInfo.adUnitId}\n";
                        break;
                    case AdStatusType.Paid:
                        msg = $"BADS: {ColoredText("display", Const.ColorGreen)}\n\tnetwork: {_curBadsInfo.network}\n\trevenue: {_curBadsInfo.revenue}\n";
                        break;
                    case AdStatusType.NotReady:
                        msg = $"BADS: {ColoredText("not ready", Const.ColorGray)}\n\t{ColoredText("---", Const.ColorGray)}\n";
                        break;
                    default:
                        msg = $"BADS: {ColoredText("waiting", Const.ColorGray)}\n\tstatus: {ColoredText($"{_curBadsInfo.status}", Const.ColorYellow)}\n";
                        break;
                }
            }
            _infoBuff.Append(msg);


            if (_curIadsInfo == null)
            {
                msg = $"IADS: {ColoredText("not ready", Const.ColorRed)}\n";
            }
            else
            {
                switch (_curIadsInfo.status)
                {
                    case AdStatusType.Loaded:
                        msg = $"IADS: {ColoredText("loaded", Const.ColorGreen)}\n\tnetwork: {_curIadsInfo.network}\n\twaterfall: {_curIadsInfo.waterfall}\n";
                        break;
                    case AdStatusType.Failed:
                        msg = $"IADS: {ColoredText("failed", Const.ColorRed)}\n\terrorCode: {_curIadsInfo.errorCode}\n";
                        break;
                    case AdStatusType.Loading:
                        msg = $"IADS: {ColoredText("loading...", Const.ColorYellow)}\n\tadId: {_curIadsInfo.adUnitId}\n";
                        break;
                    case AdStatusType.Paid:
                        msg = $"IADS: {ColoredText("paid", Const.ColorGreen)}\n\trevenue: {_curIadsInfo.revenue}\n";
                        break;
                    case AdStatusType.NotReady:
                        msg = $"IADS: {ColoredText("not ready", Const.ColorGray)}\n\t{ColoredText("---", Const.ColorGray)}\n";
                        break;
                    default:
                        msg = $"IADS: {ColoredText("waiting", Const.ColorGray)}\n\tstatus: {ColoredText($"{_curIadsInfo.status}", Const.ColorYellow)}\n";
                        break;
                }
            }
            _infoBuff.Append(msg);
            

            if (_curRadsInfo == null)
            {
                msg = $"RADS: {ColoredText("not ready", Const.ColorRed)}\n";
            }
            else
            {
                switch (_curRadsInfo.status)
                {
                    case AdStatusType.Loaded:
                        msg = $"RADS: {ColoredText("loaded", Const.ColorGreen)}\n\tnetwork: {_curRadsInfo.network}\n\twaterfall: {_curRadsInfo.waterfall}\n";
                        break;
                    case AdStatusType.Failed:
                        msg = $"RADS: {ColoredText("failed", Const.ColorRed)}\n\terrorCode: {_curRadsInfo.errorCode}\n";
                        break;
                    case AdStatusType.Loading:
                        msg = $"RADS: {ColoredText("loading...", Const.ColorYellow)}\n\tadId: {_curRadsInfo.adUnitId}\n";
                        break;
                    case AdStatusType.Paid:
                        msg = $"RADS: {ColoredText("paid", Const.ColorGreen)}\n\trevenue: {_curRadsInfo.revenue}\n";
                        break;
                    case AdStatusType.NotReady:
                        msg = $"RADS: {ColoredText("not ready", Const.ColorGray)}\n\t{ColoredText("---", Const.ColorGray)}\n";
                        break;
                    default:
                        msg = $"RADS: {ColoredText("waiting", Const.ColorGray)}\n\tstatus: {ColoredText($"{_curRadsInfo.status}", Const.ColorYellow)}\n";
                        break;
                }
            }
            _infoBuff.Append(msg);

            _infoBuff.Append($"\n Tch-001: <color=cyan>{AdService.Instance.Model.TchAD001RevValue:G8}</color>");
            _infoBuff.Append($"\n Tch-020: <color=cyan>{AdService.Instance.Model.TchAD02RevValue:G8}</color>");
            
            return _infoBuff.ToString();

        }


        private string ColoredText(string content, string hexColor = "000000")
        {
            return $"<color=#{hexColor}>{content}</color>";
        }


        #endregion
        
        #region AppLovin
        
        private void InitAdsAssets()
        {
            RemoveCallbacks();
            AddCallbacks();
            _bannerInfos = new Queue<AdStatusInfo>(AdStatusConsts.MaxInfoCount);
            _interInfos = new Queue<AdStatusInfo>(AdStatusConsts.MaxInfoCount);
            _rewardInfos = new Queue<AdStatusInfo>(AdStatusConsts.MaxInfoCount);
        }


        private void AddCallbacks()
        {

            //----------------- Banner -----------------
            AdService.Instance.OnBadsLoad += OnBannerStartLoadEvent;
            AdService.Instance.OnBadsLoaded += OnBannerAdLoadedEvent;
            AdService.Instance.OnBadsFailed += OnBannerAdLoadFailEvent;
            AdService.Instance.OnBadsPaid += OnBannerAdRevenuePaidEvent;
            AdService.Instance.OnBadsClick += OnBannerAdClickedEvent;
            //----------------- Interstitials -----------------
            AdService.Instance.OnIadsLoad += OnInterStartLoadEvent;
            AdService.Instance.OnIadsLoaded += OnInterAdLoadedEvent;
            AdService.Instance.OnIadsFailed += OnInterAdLoadFailEvent;
            AdService.Instance.OnIadsPaid += OnInterAdRevenuePaidEvent;
            AdService.Instance.OnIadsClick += OnInterAdClickedEvent;
            AdService.Instance.OnIadsClose += OnInterAdHiddenEvent;
            //----------------- Reward -----------------
            AdService.Instance.OnRadsLoad += OnRewardedStartLoad;
            AdService.Instance.OnRadsLoaded += OnRewardAdLoadedEvent;
            AdService.Instance.OnRadsFailed += OnRewardAdFailEvent;
            AdService.Instance.OnRadsPaid += OnRewardAdRevenuePaidEvent;
            AdService.Instance.OnRadsClick += OnRewardAdClickedEvent;
            AdService.Instance.OnRadsClose += OnRewardAdHiddenEvent;
        }
        
        private void RemoveCallbacks()
        {
            //----------------- Banner -----------------
            AdService.Instance.OnBadsLoad -= OnBannerStartLoadEvent;
            AdService.Instance.OnBadsLoaded -= OnBannerAdLoadedEvent;
            AdService.Instance.OnBadsFailed -= OnBannerAdLoadFailEvent;
            AdService.Instance.OnBadsPaid -= OnBannerAdRevenuePaidEvent;
            AdService.Instance.OnBadsClick -= OnBannerAdClickedEvent;
            //----------------- Interstitials -----------------
            AdService.Instance.OnIadsLoad -= OnInterStartLoadEvent;
            AdService.Instance.OnIadsLoaded -= OnInterAdLoadedEvent;
            AdService.Instance.OnIadsFailed -= OnInterAdLoadFailEvent;
            AdService.Instance.OnIadsPaid -= OnInterAdRevenuePaidEvent;
            AdService.Instance.OnIadsClick -= OnInterAdClickedEvent;
            AdService.Instance.OnIadsClose -= OnInterAdHiddenEvent;
            //----------------- Reward -----------------
            AdService.Instance.OnRadsLoad -= OnRewardedStartLoad;
            AdService.Instance.OnRadsLoaded -= OnRewardAdLoadedEvent;
            AdService.Instance.OnRadsFailed -= OnRewardAdFailEvent;
            AdService.Instance.OnRadsPaid -= OnRewardAdRevenuePaidEvent;
            AdService.Instance.OnRadsClick -= OnRewardAdClickedEvent;
            AdService.Instance.OnRadsClose -= OnRewardAdHiddenEvent;
        }
        
        //-------------- Banner ------------------
        private void OnBannerStartLoadEvent(BadsLoadEvent evt)
        {
            AddBannerInfo(CreateLoadingInfo(evt.adUnitId, AdType.Banner));
        }
        
        private void OnBannerAdLoadedEvent(BadsLoadedEvent evt)
        {
            AddBannerInfo(CreateLoadedInfo(evt.adUnitId, AdType.Banner, evt.adSource, evt.waterfallName));
        }
        
        private void OnBannerAdLoadFailEvent(BadsFailedEvent evt)
        {
            AddBannerInfo(CreateFailInfo(evt.adUnitId, AdType.Banner, evt.waterfallName, evt.errorCode));
        }

        private void OnBannerAdClickedEvent(BadsClickEvent evt)
        {
            AddBannerInfo(CreateClosedInfo(evt.adUnitId, AdType.Banner));
        }

        private void OnBannerAdRevenuePaidEvent(BadsPaidEvent evt)
        {
            AddBannerInfo(CreatePaidInfo(evt.adUnitId, AdType.Banner, evt.value, evt.adSource, evt.adPlacement));
        }

        
        //----------------- Interstitial -----------------
        private void OnInterStartLoadEvent(IadsLoadEvent evt)
        {
            AddInterInfo(CreateLoadingInfo(evt.adUnitId, AdType.Interstitial));
        }
        
        private void OnInterAdHiddenEvent(IadsCloseEvent evt)
        {
            AddInterInfo(CreateClosedInfo(evt.adUnitId, AdType.Interstitial));
        }

        private void OnInterAdClickedEvent(IadsClickEvent evt)
        {
            AddInterInfo(CreateClickedInfo(evt.adUnitId, AdType.Interstitial, evt.placement));
        }

        private void OnInterAdRevenuePaidEvent(IadsPaidEvent evt)
        {
            AddInterInfo(CreatePaidInfo(evt.adUnitId, AdType.Interstitial, evt.value, evt.adSource, evt.adPlacement));
        }

        private void OnInterAdLoadFailEvent(IadsFailedEvent evt)
        {
            AddInterInfo(CreateFailInfo(evt.adUnitId, AdType.Interstitial, evt.waterfallName, evt.errorCode));
        }
        
        private void OnInterAdLoadedEvent(IadsLoadedEvent evt)
        {
            AddInterInfo(CreateLoadedInfo(evt.adUnitId, AdType.Interstitial, evt.adSource, evt.waterfallName));
        }
        
        //----------------- Reward -----------------
        private void OnRewardedStartLoad(RadsLoadEvent evt)
        {
            AddRewardInfo(CreateLoadingInfo(evt.adUnitId, AdType.Rewarded));
        }
        
        private void OnRewardAdHiddenEvent(RadsCloseEvent evt)
        {
            AddRewardInfo(CreateClosedInfo(evt.adUnitId, AdType.Rewarded));
        }

        private void OnRewardAdClickedEvent(RadsClickEvent evt)
        {
            AddRewardInfo(CreateClickedInfo(evt.adUnitId, AdType.Rewarded, evt.placement));
        }

        private void OnRewardAdRevenuePaidEvent(RadsPaidEvent evt)
        {
            AddRewardInfo(CreatePaidInfo(evt.adUnitId, AdType.Rewarded, evt.value, evt.adSource, evt.adPlacement));
        }

        private void OnRewardAdFailEvent(RadsFailedEvent evt)
        {
            AddRewardInfo(CreateFailInfo(evt.adUnitId, AdType.Rewarded, evt.waterfallName, evt.errorCode));
        }
        
        private void OnRewardAdLoadedEvent(RadsLoadedEvent evt)
        {
            AddRewardInfo(CreateLoadedInfo(evt.adUnitId, AdType.Rewarded, evt.adSource, evt.waterfallName));
        }
    
        
        #endregion

        

    }
}