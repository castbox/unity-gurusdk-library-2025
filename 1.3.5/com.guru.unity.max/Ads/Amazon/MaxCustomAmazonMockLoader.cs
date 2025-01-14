

namespace Guru.Ads.Max
{
    using System;
    using Cysharp.Threading.Tasks;
    
    /// <summary>
    /// Mock加载器适用于编辑器
    /// </summary>
    public class MaxCustomAmazonMockLoader: ICustomAmazonLoader
    {
        public void RequestBanner(Action createMaxBanner)
        {
            _ = DelayCall(1, createMaxBanner);
        }

        public void RequestInterstitial(Action loadMaxInter)
        {
            _ = DelayCall(1, loadMaxInter);
        }

        public void RequestRewarded(Action loadMaxInter)
        {
            _ = DelayCall(1, loadMaxInter);
        }

        /// <summary>
        /// 延迟调用
        /// </summary>
        /// <param name="delaySeconds"></param>
        /// <param name="callback"></param>
        private async UniTaskVoid DelayCall(float delaySeconds, Action callback)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds));
            callback?.Invoke();
        }
    }
}