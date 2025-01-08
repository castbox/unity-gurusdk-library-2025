

namespace Guru.Ads.Max
{
    using UnityEngine;
    using AmazonAds;
    using System;
    
    internal struct AdSize
    {
        public int width;
        public int height;

        public AdSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    } 
    
    /// <summary>
    /// Amazon 广告预加载器
    /// 代码示例：https://developers.applovin.com/en/max/unity/amazon-publisher-services-integration-instructions/
    /// </summary>
    public class MaxCustomLoaderAmazon
    {
        private readonly string _maxBannerUnitId;
        private readonly string _maxInterUnitId;
        private readonly string _maxRewardedUnitId;
        
        private readonly string _apsBannerSlotId;
        private readonly string _apsInterSlotId;
        private readonly string _apsRewardedSlotId;

        private const string K_AMAZON_AD_RESPONSE = "amazon_ad_response";
        private const string K_AMAZON_AD_ERROR = "amazon_ad_error";
        
        private bool _hasBannerFistRequested = false; 
        
        private bool _hasIadsFirstLoad = false;
        
        private bool _hasRewardedFirstLoad = false;

        /// <summary>
        /// 是否可用
        /// </summary>
        private bool IsAvailable
        {
            get
            {
#if UNITY_EDITOR
                Debug.Log($"<color=orange>=== Amazon will not init on Editor ===</color>");
                return false;      
#endif
                return true;
            }
        }


        // Banner 尺寸参数
        private readonly AdSize _bannerSize;
        // Video 尺寸参数
        private readonly AdSize _videoSize;

        public MaxCustomLoaderAmazon(string apsAppId, 
            string apsBannerSlotId, string apsInterSlotId, string apsRewardedSlotId, 
            string maxBannerUnitId, string maxInterUnitId, string maxRewardedUnitId, 
            bool isDebug = false)
        {
            if (!IsAvailable) return;

            _maxBannerUnitId = maxBannerUnitId;
            _maxInterUnitId = maxInterUnitId;
            _maxRewardedUnitId = maxRewardedUnitId;
            
            _apsBannerSlotId = apsBannerSlotId;
            _apsInterSlotId = apsInterSlotId;
            _apsRewardedSlotId = apsRewardedSlotId;

            _bannerSize = new AdSize(320, 50);
            if (MaxSdkUtils.IsTablet())
            {
                _bannerSize = new AdSize(728, 90); 
            }

            _videoSize = new AdSize(320, 480);
            
            // 初始化Amazon
            Amazon.Initialize (apsAppId);
            Amazon.SetAdNetworkInfo(new AdNetworkInfo(DTBAdNetwork.MAX));
            Debug.Log($"[Ads] --- Amazon init start isDebug:{isDebug},    AmazonID:{apsAppId}");
            Amazon.EnableTesting (isDebug); // Make sure to take this off when going live.
            Amazon.EnableLogging (isDebug);
		
#if UNITY_IOS
            Amazon.SetAPSPublisherExtendedIdFeatureEnabled(true);
#endif
        }

        /// <summary>
        /// 请求 Amazon 的 Banner 广告
        /// </summary>
        /// <param name="createMaxBanner"></param>
        public void RequestAPSBanner(Action createMaxBanner)
        {
            // if (_hasBannerFistRequested)
            // {
            //     Debug.Log($"[Ads] --- Amazon Banner: Has requested! Create Max Banner directly!");
            //     createMaxBanner?.Invoke();
            //     return;
            // }

            if (!IsAvailable)
            {
                Debug.Log($"[Ads] --- Amazon Banner: Is Not Available!");
                return;
            }
            
            Debug.Log($"[Ads] --- Amazon banner start load: {_apsBannerSlotId}");
            var apsBanner = new APSBannerAdRequest(_bannerSize.width, _bannerSize.height, _apsBannerSlotId);
            apsBanner.onSuccess += (adResponse) =>
            {
                Debug.Log($"[Ads] --- Amazon Banner Load Success ---");
                MaxSdk.SetBannerLocalExtraParameter(_maxBannerUnitId, 
                    K_AMAZON_AD_RESPONSE, 
                    adResponse.GetResponse());
                // OnBannerRequestOver?.Invoke(true, _firstLoadBanner);
                createMaxBanner?.Invoke();
            };
            
            apsBanner.onFailedWithError += (adError) =>
            {
                Debug.Log($"[Ads] --- Amazon Banner Load Fail: [{adError.GetCode()}] {adError.GetMessage()}");
                MaxSdk.SetBannerLocalExtraParameter(_maxBannerUnitId, 
                    K_AMAZON_AD_ERROR, 
                    adError.GetAdError());
                // OnBannerRequestOver?.Invoke(false, _firstLoadBanner);
                createMaxBanner?.Invoke();
            };
            
            apsBanner.LoadAd();
            // _hasBannerFistRequested = true;
        }


        /// <summary>
        /// 请求 Amazon 的 Inter 广告
        /// </summary>
        /// <param name="loadMaxInter"></param>
        public void RequestInterstitial(Action loadMaxInter)
        {
            if (!IsAvailable)
            {
                Debug.Log($"[Ads] --- Amazon INTER: Is Not Available!");
                return;
            }
            
            // 首次启动注入渠道参数
            if (_hasIadsFirstLoad)
            {
                Debug.Log($"[Ads] --- Amazon INTER: _hasIadsFirstLoad!");
                loadMaxInter?.Invoke();
                return;
            }
            
            Debug.Log($"[Ads] --- Amazon INTER start load: {_apsInterSlotId}");
            var interstitialAd = new APSInterstitialAdRequest(_apsInterSlotId);
            interstitialAd.onSuccess += (adResponse) =>
            {
                Debug.Log($"[Ads] --- Amazon INTER Load Success ---");
                MaxSdk.SetInterstitialLocalExtraParameter(_maxInterUnitId, 
                    K_AMAZON_AD_RESPONSE,
                    adResponse.GetResponse());
                // OnInterstitialRequestOver?.Invoke(true, true);
                loadMaxInter?.Invoke();
            };
            interstitialAd.onFailedWithError += (adError) =>
            {
                Debug.Log($"[Ads] --- Amazon INTER Load Fail: [{adError.GetCode()}] {adError.GetMessage()}");
                MaxSdk.SetInterstitialLocalExtraParameter(_maxInterUnitId, 
                    K_AMAZON_AD_ERROR, 
                    adError.GetAdError());
                // OnInterstitialRequestOver?.Invoke(false, true); // 不成功则一直请求Amazon广告
                loadMaxInter?.Invoke();
            };
            
            _hasIadsFirstLoad = true;
            interstitialAd.LoadAd();
        }


        /// <summary>
        /// 请求 Amazon 的 RV 广告
        /// </summary>
        /// <param name="loadMaxRewarded"></param>
        public void RequestRewarded(Action loadMaxRewarded)
        {
            if (!IsAvailable)
            {
                Debug.Log($"[Ads] --- Amazon RV: Is Not Available!");
                return;
            }
            
            if (_hasRewardedFirstLoad)
            {
                Debug.Log($"[Ads] --- Amazon RV: _hasRewardedFirstLoad!");
                loadMaxRewarded?.Invoke();
                return;
            }
            
            Debug.Log($"[Ads] --- Amazon Reward start load: {_apsRewardedSlotId}");
            var rewardedVideoAd = new APSVideoAdRequest(_videoSize.width, _videoSize.height, _apsRewardedSlotId);
            rewardedVideoAd.onSuccess += (adResponse) =>
            {
                Debug.Log($"[Ads] --- Amazon Reward Load Success ---");
                MaxSdk.SetRewardedAdLocalExtraParameter(_maxRewardedUnitId, 
                    K_AMAZON_AD_RESPONSE,
                    adResponse.GetResponse());
                // OnRewardRequestOver?.Invoke(true, true);
                loadMaxRewarded?.Invoke();
            };
            rewardedVideoAd.onFailedWithError += (adError) =>
            {
                Debug.Log($"[Ads] --- Amazon Reward Load Fail: [{adError.GetCode()}] {adError.GetMessage()}");
                MaxSdk.SetRewardedAdLocalExtraParameter(_maxRewardedUnitId, 
                    K_AMAZON_AD_ERROR, 
                    adError.GetAdError());
                // OnRewardRequestOver?.Invoke(false, true);  // 不成功则一直请求Amazon广告
                loadMaxRewarded?.Invoke();
            };
            rewardedVideoAd.LoadAd();
            _hasRewardedFirstLoad = true;
        }


    }
}