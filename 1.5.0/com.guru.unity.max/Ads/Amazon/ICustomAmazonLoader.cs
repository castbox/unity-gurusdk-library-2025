namespace Guru.Ads.Max
{
    using System;
    
    public interface ICustomAmazonLoader
    {
        void RequestBanner(Action createMaxBanner);
        void RequestInterstitial(Action loadMaxInter);
        void RequestRewarded(Action loadMaxRewarded);
    }
}