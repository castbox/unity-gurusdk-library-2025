namespace Guru.Ads
{
    using System.Collections.Generic;

    // 广告时间监听器
    public interface IAdEventObserver
    {
        // void OnAdImpression( );

        // BANNER
        void OnEventBadsLoad(BadsLoadEvent evt);
        void OnEventBadsLoaded(BadsLoadedEvent evt);
        void OnEventBadsFailed(BadsFailedEvent evt);
        void OnEventBadsImp(BadsImpEvent evt);
        void OnEventBadsHide(BadsHideEvent evt);
        void OnEventBadsClick(BadsClickEvent evt);
        void OnEventBadsPaid(BadsPaidEvent evt);
        // INTER
        void OnEventIadsLoad(IadsLoadEvent evt);
        void OnEventIadsLoaded(IadsLoadedEvent evt);
        void OnEventIadsFailed(IadsFailedEvent evt);
        void OnEventIadsImp(IadsImpEvent evt);
        void OnEventIadsClick(IadsClickEvent evt);
        void OnEventIadsClose(IadsCloseEvent evt);
        void OnEventIadsPaid(IadsPaidEvent evt);
        // REWARDED
        void OnEventRadsLoad(RadsLoadEvent evt);
        void OnEventRadsLoaded(RadsLoadedEvent evt);
        void OnEventRadsFailed(RadsFailedEvent evt);
        void OnEventRadsImp(RadsImpEvent evt);
        void OnEventRadsClick(RadsClickEvent evt);
        void OnEventRadsClose(RadsCloseEvent evt);
        void OnEventRadsRewarded(RadsRewardedEvent evt);
        void OnEventRadsPaid(RadsPaidEvent evt);
        
    }
}