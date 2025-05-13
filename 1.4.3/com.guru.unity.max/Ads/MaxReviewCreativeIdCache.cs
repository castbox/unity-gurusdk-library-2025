
namespace Guru.Ads.Max
{
    using System.Collections.Generic;
    /// <summary>
    /// MaxReviewedCreativeId 专用缓存
    /// </summary>
    internal class MaxReviewCreativeIdCache
    {
        private readonly Dictionary<string, string> _cache;
        private readonly string[] _keys;
        private int _currentIndex;
        private readonly int _capacity;

        public MaxReviewCreativeIdCache(int capacity = 10)
        {
            _capacity = capacity;
            _keys = new string[capacity];
            _cache = new Dictionary<string, string>(capacity);
            _currentIndex = 0;
        }


        // 创建数据 Key
        private string BuildKey(MaxSdk.AdInfo adInfo)
        {
            return $"{adInfo.AdUnitIdentifier}_{adInfo.NetworkName}_{adInfo.NetworkPlacement}_{adInfo.CreativeIdentifier}_{adInfo.Revenue}";
        }


        /// <summary>
        /// 添加 reviewedCreativeId
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        /// <param name="reviewedCreativeId"></param>
        public void AddOrUpdate(MaxSdk.AdInfo adInfo, string reviewedCreativeId)
        {
            var key = BuildKey(adInfo);
            if (_cache.ContainsKey(key))
            {
                _cache[key] = reviewedCreativeId;
            }
            else
            {
                if (_cache.Count >= _capacity)
                {
                    // Remove the oldest entry
                    string oldestKey = _keys[_currentIndex];
                    if (_cache.ContainsKey(oldestKey))
                    {
                        _cache.Remove(oldestKey);
                    }
                }

                _cache[key] = reviewedCreativeId;
                _keys[_currentIndex] = key;
                _currentIndex = (_currentIndex + 1) % _capacity;
            }
        }

        /// <summary>
        /// 获取 reviewedCreativeId
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        /// <returns></returns>
        public string GetReviewedCreativeId(MaxSdk.AdInfo adInfo)
        {
            var key = BuildKey(adInfo);
            return _cache.GetValueOrDefault(key);
        }
        
    }
}