namespace Guru
{
    using System;
    using Cysharp.Threading.Tasks;
    
    public class UserLTPropertyManager: ILTPropertyManager
    {
        private const string TAG = "[LT]";
        private const int DAILY_ACTIVE_CHECK_INTERVAL = 1; // 日活检测间隔
        private const double DEBUG_ACTIVE_SECONDS = 15; // 测试间隔秒数
        
        private readonly ILTPropertyDataHolder _dataHolder;
        private readonly ILTPropertyReporter _reporter;
        private bool _isOnDelayChecking = false;
        private UniTaskCompletionSource _delayCompletionSource;
        private DateTime _targetTime;
        private bool _debugMode;

        /// <summary>
        /// LT 属性管理器
        /// </summary>
        /// <param name="dataHolder"></param>
        /// <param name="reporter"></param>
        public UserLTPropertyManager(ILTPropertyDataHolder dataHolder, ILTPropertyReporter reporter)
        {
            _reporter = reporter;
            _dataHolder = dataHolder;
            _debugMode = false;
            // 初始化的时候先上报第一次的 LT 数值
            _reporter?.ReportUserLT(_dataHolder.LT);
            
            CheckDailyActivity().Forget();
        }

        /// <summary>
        /// 检查日活次数
        /// </summary>
        private async UniTask CheckDailyActivity()
        {
            var isFirstActive = _dataHolder.LastActiveTime.Year == 1970;
            var currentTime = DateTime.UtcNow;
            double debugSeconds = _debugMode ? DEBUG_ACTIVE_SECONDS : 0;

            if (isFirstActive)
            {
                _dataHolder.LastActiveTime = currentTime;
            }
            else if (IsActiveAvailable(currentTime, debugSeconds))
            {
                // 延迟 N 秒检测用户活跃
                await UniTask.Delay(TimeSpan.FromSeconds(DAILY_ACTIVE_CHECK_INTERVAL));
                _dataHolder.LastActiveTime = currentTime;
                _dataHolder.LT++;
                
                LogI($" --- Set LT:{ _dataHolder.LT}  and active time {currentTime:G}");
                _reporter?.ReportUserLT(_dataHolder.LT);
            }

            // 预定换天后再进行检测
            DateTime nextDate = DateTime.UtcNow.Date.AddDays(1);

            if (debugSeconds > 0)
            {
                nextDate = DateTime.UtcNow.AddSeconds(debugSeconds + 1);
            }

            LogI($" --- Set next check time: {nextDate:G}");
            StartNextDelayCheck(nextDate).Forget(); // 预定下一次日活检测
        }

        private bool IsActiveAvailable(DateTime currentTime, double debugSeconds = 0)
        {
            if (debugSeconds > 0)
            {
                // 测试模式下， 直接按秒来进行判断
                return (currentTime - _dataHolder.LastActiveTime).TotalSeconds > debugSeconds;
            }

            // 正式模式下， 按照跨天计算
            var timeDiff = currentTime.Date - _dataHolder.LastActiveTime.Date;
            return timeDiff.Days > 0;
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
            
            try
            {
                var delaySpan = targetTime - DateTime.UtcNow;
                LogI($"--- Waiting delaySpan: {delaySpan.TotalSeconds}");
                var delayTask = UniTask.Delay(delaySpan, DelayType.UnscaledDeltaTime);
                await UniTask.WhenAny(delayTask, _delayCompletionSource.Task);
                CheckDailyActivity().Forget();
            }
            finally
            {
                _isOnDelayChecking = false;
                _delayCompletionSource = null;
            }
        }
        
        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="message"></param>
        private void LogI(string message)
        {
            if (!_debugMode) return;
            UnityEngine.Debug.Log($"{TAG} {message}");
        }
        
        /// <summary>
        /// 应用程序暂停
        /// </summary>
        /// <param name="pauseStatus"></param>
        public void OnApplicationPause(bool pauseStatus)
        {
            if (!_isOnDelayChecking || _delayCompletionSource == null) return;

            LogI($" --- App Pause status: {pauseStatus}");
            if (!pauseStatus) // 应用程序恢复
            {
                LogI($" --- Check Now Time: {DateTime.UtcNow:G} --> {_targetTime:G}");
                // 检查是否已经超过目标时间
                if (DateTime.UtcNow >= _targetTime)
                {
                    _delayCompletionSource.TrySetResult();
                }
            }
        }
        
        /// <summary>
        /// 设置调试模式
        /// </summary>
        /// <param name="flag"></param>
        public void SetDebugMode(bool flag)
        {
            _debugMode = flag;
            UnityEngine.Debug.Log($"{TAG} --- set DebugMode: {flag}");

            if (_debugMode){
                if (_isOnDelayChecking && _delayCompletionSource != null)
                {
                    _delayCompletionSource.TrySetResult();
                }
                else
                {
                    CheckDailyActivity().Forget();
                }
            }
        }

    }
}