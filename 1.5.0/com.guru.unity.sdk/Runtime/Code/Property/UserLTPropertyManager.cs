namespace Guru
{
    using System;
    using Cysharp.Threading.Tasks;
    
    public class UserLTPropertyManager
    {
        private const int DAILY_ACTIVE_CHECK_INTERVAL = 1; // 日活检测间隔
        private readonly ILTPropertyDataHolder _dataHolder;
        private readonly ILTPropertyReporter _reporter;


        /// <summary>
        /// LT 属性管理器
        /// </summary>
        /// <param name="dataHolder"></param>
        /// <param name="reporter"></param>
        public UserLTPropertyManager(ILTPropertyDataHolder dataHolder, ILTPropertyReporter reporter)
        {
            _reporter = reporter;
            _dataHolder = dataHolder;
            // 初始化的时候先上报第一次的 LT 数值
            _reporter?.ReportUserLT(_dataHolder.LT);
            
            CheckDailyActivity().Forget();
        }

        /// <summary>
        /// 检查日活次数
        /// </summary>
        private async UniTask CheckDailyActivity()
        {
            var currentTime = DateTime.UtcNow;
            var timeDiff = currentTime.Date - _dataHolder.LastActiveTime.Date;
            if (timeDiff.Days > 0)
            {
                // 延迟 N 秒检测用户活跃
                await UniTask.Delay(TimeSpan.FromSeconds(DAILY_ACTIVE_CHECK_INTERVAL));
                _dataHolder.LastActiveTime = currentTime;
                _dataHolder.LT++;
                _reporter?.ReportUserLT(_dataHolder.LT);
            }

            SetupNextDailyActivity().Forget(); // 预定下一次日活检测
        }

        /// <summary>
        /// 设置下一次日活检测
        /// </summary>
        private async UniTask SetupNextDailyActivity()
        {
            // 获取当天的午夜时间
            DateTime startOfDay = DateTime.UtcNow;
            // 获取第二天的午夜时间
            DateTime endOfDay = startOfDay.Date.AddDays(1);
            // 计算从当天午夜到第二天午夜的时间跨度
            TimeSpan timeSpan = endOfDay - startOfDay; 
            
            // 预定换天后再进行检测
            await UniTask.Delay(timeSpan, DelayType.Realtime);
            
            CheckDailyActivity().Forget();
        }
    }
}