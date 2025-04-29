namespace Guru
{
    using System;
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    
    public class EditorLTPropertyManager: ILTPropertyManager
    {
        private const string TAG = "[LT]";
        private static EditorLTPropertyManager _instance;
        public static EditorLTPropertyManager Instance => _instance;
        
        private ILTPropertyReporter _reporter;
        private ILTPropertyDataHolder _dataHolder;
        private bool _isOnDelayChecking = false;
        private UniTaskCompletionSource _delayCompletionSource;
        private DateTime _targetTime;
        
        
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
            var isFirstActive = _dataHolder.LastActiveTime.Year == 1970;
            var timeDiff = currentTime.Date - _dataHolder.LastActiveTime.Date;
            LogI($" --- CheckDailyActivity timeDiff:{ timeDiff:G}  and LastActiveTime {_dataHolder.LastActiveTime:G}");
            if (isFirstActive)
            {
                _dataHolder.LastActiveTime = currentTime;
            }
            else if (timeDiff.Days > 0)
            {
                // 延迟 N 秒检测用户活跃
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                _dataHolder.LastActiveTime = currentTime;
                _dataHolder.LT++;
                
                LogI($" --- Set LT:{ _dataHolder.LT}  and active time {currentTime:G}");
                _reporter?.ReportUserLT(_dataHolder.LT);
            }

        }

        /// <summary>
        /// 设置下一次日活检测
        /// </summary>
        private async UniTask StartNextDelayCheck(DateTime targetTime)
        {
            if (_isOnDelayChecking) return;
            _isOnDelayChecking = true;
            _delayCompletionSource = new UniTaskCompletionSource();
            _targetTime = targetTime;
            
            var delaySpan = targetTime -GetNow;
            LogI($"--- delaySpan: {delaySpan.TotalSeconds}");
            try
            {
                // 预定换天后再进行检测
                LogI($"--- waiting delaySpan: {delaySpan.TotalSeconds}");
                var delayTask = UniTask.Delay(delaySpan);
                await UniTask.WhenAny(delayTask, _delayCompletionSource.Task);
                CheckDailyActivity(GetNow).Forget();
            }
            finally
            {
                _isOnDelayChecking = false;
                _delayCompletionSource = null;
            }
        }
        
        public void OnApplicationPause(bool pauseStatus)
        {
            if (!_isOnDelayChecking || _delayCompletionSource == null) return;

            LogI($" --- Get App Pause status: {pauseStatus}");
            if (!pauseStatus) // 应用程序恢复
            {
                // 检查是否已经超过目标时间
                if (GetNow >= _targetTime)
                {
                    LogI($" --- CheckTime: {GetNow:G}  --> {_targetTime:G} :: _delayCompletionSource:{_delayCompletionSource}");
                    _delayCompletionSource.TrySetResult();
                }
            }
        }

        private void LogI(string message)
        {
            UnityEngine.Debug.Log($"{TAG} {message}");
        }
        
        private DateTime GetNow => DateTime.Now;
        
        /// <summary>
        /// 设置调试模式
        /// </summary>
        /// <param name="flag"></param>
        public void SetDebugMode(bool flag)
        {
            // 开启测试模式：
            LogI($"--- Set Debug Mode: {flag}");
        }
        
#if UNITY_EDITOR

        /// <summary>
        /// 开始单元测试
        /// </summary>
        public void StartUnitTest()
        {
            UnityEngine.Debug.Log("----- Start LT Unit Test -----");
            
            AsyncRunUnitTest().Forget();
        }


        private async UniTaskVoid AsyncRunUnitTest()
        {
            var mockLastActiveTime = GetNow.AddDays(-1);
            LogI($"#1 set mockLastActiveTime: {mockLastActiveTime:G}");
            _dataHolder.LastActiveTime = mockLastActiveTime;
            
            LogI($"#2 CheckDailyActivity");
            await CheckDailyActivity(GetNow);

            var nextDate = GetNow.AddSeconds(5); //下次的检查点时间
            LogI($"#3 Set nextDate: {GetNow:G} -> {nextDate:G}");
            await StartNextDelayCheck(nextDate);
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