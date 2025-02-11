


namespace Guru
{
    using System;
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    
    public class EditorLTPropertyManager
    {
        private const string TAG = "[LT]";
        private static EditorLTPropertyManager _instance;
        public static EditorLTPropertyManager Instance => _instance;
        
        private ILTPropertyReporter _reporter;
        private ILTPropertyDataHolder _dataHolder;
        public EditorLTPropertyManager(ILTPropertyReporter reporter)
        {
            _reporter = reporter;
            _dataHolder = new EditorLTData();
            _instance = this;
            
            _reporter.ReportUserLT( _dataHolder.LT);
        }
        
        /// <summary>
        /// 检查日活次数
        /// </summary>
        private async UniTask CheckDailyActivity(DateTime currentTime)
        {
            var timeDiff = currentTime.Date - _dataHolder.LastActiveTime.Date;
            if (timeDiff.Days > 0)
            {
                // 延迟 N 秒检测用户活跃
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                _dataHolder.LastActiveTime = currentTime;
                _dataHolder.LT++;
                _reporter?.ReportUserLT(_dataHolder.LT);
            }

        }

        /// <summary>
        /// 设置下一次日活检测
        /// </summary>
        private async UniTask SetupNextDailyActivity(DateTime currentTime)
        {
            // 获取当天的午夜时间
            DateTime startOfDay = currentTime;
            // 获取第二天的午夜时间
            DateTime endOfDay = startOfDay.Date.AddDays(1);
            // 计算从当天午夜到第二天午夜的时间跨度
            TimeSpan timeSpan = endOfDay - startOfDay; 
            
            // 预定换天后再进行检测
            await UniTask.Delay(timeSpan,  DelayType.Realtime);

            CheckDailyActivity(endOfDay).Forget();
        }


#if UNITY_EDITOR

        /// <summary>
        /// 开始单元测试
        /// </summary>
        public void StartUnitTest()
        {
            UnityEngine.Debug.Log("----- Start LT Unit Test -----");
            
            var mockLastActiveTime = DateTime.Now.AddDays(-1);
            LogI($"#1 set mockLastActiveTime: {mockLastActiveTime:g}");
            _dataHolder.LastActiveTime = mockLastActiveTime;
            
            LogI($"#2 CheckDailyActivity");
            CheckDailyActivity(DateTime.Now).Forget();

            mockLastActiveTime = DateTime.Now.Date.AddSeconds(86390); // 23:59:55
            LogI($"#3 Reset LastActiveTime: {mockLastActiveTime:g}");
            _dataHolder.LastActiveTime = mockLastActiveTime;
            
            LogI($"#4 CheckAuto Report LT");
            SetupNextDailyActivity(mockLastActiveTime).Forget();
        }


        private void LogI(string message)
        {
            UnityEngine.Debug.Log($"{TAG} {message}");
        }


#endif
    }

    internal class EditorLTData: ILTPropertyDataHolder
    {
        public DateTime LastActiveTime { get; set; }
        public int LT { get; set; } = 1;


        internal EditorLTData()
        {
            
        }
    }


}