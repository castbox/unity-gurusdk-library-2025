using System;

namespace Guru
{
    public class AdStatusModel
    {


        public string monitorInfo;


        public int bannerTotalCount = 0;
        public int bannerSuccessCount = 0;
        public int bannerFailCount = 0;
        
        public int interstitialTotalCount = 0;
        public int interstitialSuccessCount = 0;
        public int interstitialFailCount = 0;
        
        public int rewardedTotalCount = 0;
        public int rewardedSuccessCount = 0;
        public int rewardedFailCount = 0;
        
        public void AddBannerCount(bool success)
        {
            bannerTotalCount++;
            if (success)
            {
                bannerSuccessCount++;
            }
            else
            {
                bannerFailCount++;
            }
            
        }
        
        public void AddInterCount(bool success)
        {
            interstitialTotalCount++;
            if (success)
            {
                interstitialSuccessCount++;
            }
            else
            {
                interstitialFailCount++;
            }
        }
        
        public void AddRewardCount(bool success)
        {
            rewardedTotalCount++;
            if (success)
            {
                rewardedSuccessCount++;
            }
            else
            {
                rewardedFailCount++;
            }
            
        }


        
    }
}