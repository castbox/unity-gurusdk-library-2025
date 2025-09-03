using System.Collections.Generic;

namespace Guru
{
    public partial class GuruSDK
    {

        
        private void PrepareCustomEventDrivers()
        {
            // ------------ 在此处添加所有自动添加的三方打点器 ---------------------
            // 
            
#if GURU_ADJUST
            AddAdjustDriver();
#endif 
            
            //
            // ------------ 在此处添加所有自动添加的三方打点器 ---------------------
            
            // 预初始化打点器
            Analytics.PrepareCustomDrivers();
        }



        // ------------------- Adjust --------------------------

#if GURU_ADJUST
        private AdjustEventDriver _adjustEventDriver;
        
        /// <summary>
        /// 仅在 Adjust 激活的状态下有效
        /// </summary>
        private void AddAdjustDriver()
        {
            // 自动添加 AdjustEventDriver
            _adjustEventDriver = new AdjustEventDriver(new AdjustProfile(
                _adjustToken ?? string.Empty,
                _adjustEventMap ?? new Dictionary<string, string>(),
                _initConfig.AdjustDeferredReportAdRevenueEnabled,
                0,
                PlatformUtil.IsDebug(),
                _initConfig.OnAdjustDeeplinkCallback
            ));
            
            Analytics.AddCustomDriver(_adjustEventDriver);
        }

        private void UpdateAdjustDelayStrategy()
        {

            // Adjust 延迟时间
            if (_remoteConfigManager.TryGetRemoteData(AdjustService.REMOTE_DELAY_TIME_KEY, out var data))
            {
                var dataSource = data.Source switch
                {
                    ValueSource.Local => DelayMinutesSource.Local,
                    ValueSource.Remote => DelayMinutesSource.RemoteConfig,
                    _ => DelayMinutesSource.Default
                };
                _adjustEventDriver.SetAdRevDelayMinutes(data.GetValue(AdjustService.DEFAULT_DELAY_MINUTES), dataSource);
            }
        }

        
#endif
        
        
    }
}