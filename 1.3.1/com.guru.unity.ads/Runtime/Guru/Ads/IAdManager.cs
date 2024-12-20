
namespace Guru.Ads
{
    using UnityEngine;

    public interface IAdManager
    {
        // 初始化
        bool IsReady();

        string GetMediationName();
        string GetLogTag();
        
        // 生命周期
        void SetAppPause(bool paused);
        
        // 激活去广告
        void EnableNoAds();
        
        // Banner
        void ShowBanner(string placement = "");
        void HideBanner();
        bool IsBannerVisible();
        void SetBannerAdUnitId(string adUnitId);
        Rect GetBannerLayout();
        
        // Interstitial
        void LoadInterstitial();
        bool IsInterstitialReady();
        void ShowInterstitial(string placement = "");
        void SetInterstitialAdUnitId(string adUnitId);
        
        // Rewarded
        void LoadRewarded();
        bool IsRewardedReady();
        void ShowRewarded(string placement = "");
        void SetRewardedAdUnitId(string adUnitId);
        
    }
}