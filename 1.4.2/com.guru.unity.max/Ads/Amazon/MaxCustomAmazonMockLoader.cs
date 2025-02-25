

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
            DelayCall(1, createMaxBanner).Forget();
        }

        public void RequestInterstitial(Action loadMaxInter)
        {
            DelayCall(1, loadMaxInter).Forget();
        }

        public void RequestRewarded(Action loadMaxInter)
        {
            DelayCall(1, loadMaxInter).Forget();
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