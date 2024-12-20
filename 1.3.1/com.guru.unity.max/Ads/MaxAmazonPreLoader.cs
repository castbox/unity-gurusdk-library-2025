

namespace Guru.Ads.Max
{
    using UnityEngine;
    using AmazonAds;
    
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
    
    public class MaxAmazonPreLoader
    {

        private readonly string _badsUnitId;
        private readonly string _iadsUnitId;
        private readonly string _radsUnitId;
        
        private APSBannerAdRequest _badsRequest;
        private bool _isBadsRequestSuccess = false; 
        
        private APSVideoAdRequest _iadsRequest;
        private bool _isIadsRequestSuccess = false;
        
        private APSVideoAdRequest _radsRequest;
        private bool _isRewardedRequestSuccess = false;

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

        public MaxAmazonPreLoader(string appId, string badsUnitId, string iadsUnitId, string radsUnitId, bool isDebug = false)
        {
            if (!IsAvailable) return;
            
            _badsUnitId = badsUnitId;
            _iadsUnitId = iadsUnitId;
            _radsUnitId = radsUnitId;

            _bannerSize = new AdSize(320, 50);
            _videoSize = new AdSize(320, 480);
            
            // 初始化Amazon
            Amazon.Initialize (appId);
            Amazon.SetAdNetworkInfo(new AdNetworkInfo(DTBAdNetwork.MAX));
            Debug.Log($"[Ads] --- Amazon init start isDebug:{isDebug},    AmazonID:{appId}");
            Amazon.EnableTesting (isDebug); // Make sure to take this off when going live.
            Amazon.EnableLogging (isDebug);
		
#if UNITY_IOS
            Amazon.SetAPSPublisherExtendedIdFeatureEnabled(true);
#endif
        }


        public void PreLoadBanner()
        {
            if (_isBadsRequestSuccess) return;

            if (!IsAvailable) return;
            
            Debug.Log($"--- Amazon banner start load ---");
            if (_badsRequest != null) _badsRequest.DestroyFetchManager();
            _badsRequest = new APSBannerAdRequest(_bannerSize.width, _bannerSize.height, _badsUnitId);
            _badsRequest.onSuccess += (adResponse) =>
            {
                Debug.Log($"--- Amazon Banner Load Success ---");
                MaxSdk.SetBannerLocalExtraParameter(_badsUnitId, 
                    "amazon_ad_response", 
                    adResponse.GetResponse());
                // OnBannerRequestOver?.Invoke(true, _firstLoadBanner);
                _isBadsRequestSuccess = true;
            };
            
            _badsRequest.onFailedWithError += (adError) =>
            {
                Debug.Log($"--- Amazon Banner Load Fail: [{adError.GetCode()}] {adError.GetMessage()}");
                MaxSdk.SetBannerLocalExtraParameter(_badsUnitId, 
                    "amazon_ad_error", 
                    adError.GetAdError());
                // OnBannerRequestOver?.Invoke(false, _firstLoadBanner);
            };
            
            _badsRequest.LoadAd();
        }



        public void PreLoadInterstitial()
        {
            // 首次启动注入渠道参数
            if (_isIadsRequestSuccess) return;
            
            if (!IsAvailable) return;
            
            Debug.Log($"--- Amazon INTER start load ---");
            _iadsRequest = new APSVideoAdRequest(_videoSize.width, _videoSize.height, _iadsUnitId);
            _iadsRequest.onSuccess += (adResponse) =>
            {
                Debug.Log($"--- Amazon INTER Load Success ---");
                MaxSdk.SetInterstitialLocalExtraParameter(_iadsUnitId, 
                    "amazon_ad_response",
                    adResponse.GetResponse());
                // OnInterstitialRequestOver?.Invoke(true, true);
                _isIadsRequestSuccess = true;
            };
            _iadsRequest.onFailedWithError += (adError) =>
            {
                Debug.Log($"--- Amazon INTER Load Fail: [{adError.GetCode()}] {adError.GetMessage()}");
                MaxSdk.SetInterstitialLocalExtraParameter(_iadsUnitId, 
                    "amazon_ad_error", 
                    adError.GetAdError());
                // OnInterstitialRequestOver?.Invoke(false, true); // 不成功则一直请求Amazon广告
            };
            
            _iadsRequest.LoadAd();
        }



        public void PreLoadRewarded()
        {
            if (_isRewardedRequestSuccess) return;
            
            if (!IsAvailable) return;
            
            Debug.Log($"--- Amazon Reward start load ---");
            _radsRequest = new APSVideoAdRequest(_videoSize.width, _videoSize.height, _radsUnitId);
            _radsRequest.onSuccess += (adResponse) =>
            {
                Debug.Log($"--- Amazon Reward Load Success ---");
                MaxSdk.SetRewardedAdLocalExtraParameter(_radsUnitId, 
                    "amazon_ad_response",
                    adResponse.GetResponse());
                // OnRewardRequestOver?.Invoke(true, true);
                _isRewardedRequestSuccess = true;
            };
            _radsRequest.onFailedWithError += (adError) =>
            {
                Debug.Log($"--- Amazon Reward Load Fail: [{adError.GetCode()}] {adError.GetMessage()}");
                MaxSdk.SetRewardedAdLocalExtraParameter(_radsUnitId, 
                    "amazon_ad_error", 
                    adError.GetAdError());
                // OnRewardRequestOver?.Invoke(false, true);  // 不成功则一直请求Amazon广告
            };
            _radsRequest.LoadAd();
            
        }


    }
}