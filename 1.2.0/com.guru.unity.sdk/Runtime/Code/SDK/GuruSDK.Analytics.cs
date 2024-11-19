
namespace Guru
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// 打点管理
    /// </summary>
    public partial class GuruSDK
    {
        #region 通用接口
        //TODO: 需要有一个通用的 IEventData 的接口， 需要实现 getName, getData, getSetting, getPriority 等方法
        //TODO: Analytics.Track 的参数改为 IEventData

        /// <summary>
        /// 自定义事件打点
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        /// <param name="setting"></param>
        /// <param name="priority"></param>
        public static void LogEvent(string eventName, Dictionary<string, dynamic> data = null, 
            EventSetting setting = null,
            EventPriority priority = EventPriority.Unknown)
        {
            if(priority == EventPriority.Unknown) priority = GetEventPriority(eventName);
            Analytics.TrackEvent(new TrackingEvent(eventName, data, setting, priority));
        }

        public static void SetScreen(string screen, string extra = "")
        {
            if (!IsInitialSuccess)
            {
                UnityEngine.Debug.LogWarning($"{LOG_TAG} :: SetScreen {screen} can not be set before SDK init!");
                return;
            }
            Analytics.SetCurrentScreen(screen, extra);
        }

        #endregion

        #region 中台通用属性

        /// <summary>
        /// 上报 SDK 基础属性
        /// </summary>
        private void ReportBasicUserProperties()
        {
            // 上报中台基础属性
            Analytics.SetUid(IPMConfig.IPM_UID);
            Analytics.SetDeviceId(IPMConfig.IPM_DEVICE_ID);
            Analytics.SetIsIapUser(Model.IsIapUser); // 从 Model 中注入打点属性初始值
            Analytics.SetFirstOpenTime(IPMConfig.FIRST_OPEN_TIME);
            Analytics.SetUserCreatedTime(IPMConfig.USER_CREATED_TIMESTAMP);
            Analytics.SetIDFA(IPMConfig.IDFA);
            Analytics.SetIDFV(IPMConfig.IDFV);
            Analytics.SetAndroidId(IPMConfig.ANDROID_ID);
        }

        #endregion
        
        #region 设置用户属性

        /// <summary>
        /// 设置用户属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetUserProperty(string key, string value)
        {
            Analytics.SetUserProperty(key, value);
        }

        private static void SetUID(string uid)
        {
            Analytics.SetUid(uid);
            // Crashlytics 设置 uid
            CrashlyticsAgent.SetUserId(uid);
        }

        public static void SetUserBLevel(int bLevel)
        {
            Analytics.SetBLevel(bLevel);
            Model.BLevel = bLevel;
        }
        
        public static void SetUserBPlay(int bPlay)
        {
            Analytics.SetBPlay(bPlay);
            Model.BPlay = bPlay;
        }
        
        /// <summary>
        /// 上报用户全部的 Coin (当前值)
        /// </summary>
        /// <param name="coins"></param>
        public static void SetUserCoin(int coins)
        {
            SetUserProperty(Consts.PropertyCoin, $"{coins}");        
        }
        
        /// <summary>
        /// 上报用户免费金币的 数量 (累加值)
        /// </summary>
        /// <param name="freeCoins"></param>
        public static void SetUserNonIapCoin(int freeCoins)
        {
            SetUserProperty(Consts.PropertyNonIAPCoin, $"{freeCoins}");        
        }
        
        /// <summary>
        /// 上报用户付费金币的 数量 (累加值)
        /// </summary>
        /// <param name="paidCoins"></param>
        public static void SetUserIapCoin(int paidCoins)
        {
            SetUserProperty(Consts.PropertyIAPCoin, $"{paidCoins}");        
        }
        
        public static void SetUserExp(int exp)
        {
            SetUserProperty(Consts.PropertyExp, $"{exp}");        
        }
        
        public static void SetUserHp(int hp)
        {
            SetUserProperty(Consts.PropertyHp, $"{hp}");        
        }

        public static void SetUserGrade(int grade)
        {
            SetUserProperty(Consts.PropertyGrade, $"{grade}");        
        }

        public static void SetUserIsIAP(bool isIapUser)
        {
            Analytics.SetIsIapUser(isIapUser);
        }

        public static void SetATTStatus(string status)
        {
            Analytics.SetAttStatus(status);
        }
        
        
        #endregion
        
        #region 游戏打点

        /// <summary>
        /// 游戏启动打点 (level_start)
        /// </summary>
        /// <param name="levelId">关卡Id</param>
        /// <param name="levelName">关卡名称: main_01_9001,  daily_challenge_81011</param>
        /// <param name="levelType">关卡类型: 主线:main</param>
        /// <param name="itemId">配置/谜题/图片/自定义Id: 101120</param>
        /// <param name="startType">关卡开始类型: play：开始游戏；replay：重玩；continue：继续游戏</param>
        /// <param name="isReplay">是否重新开始: true/false</param>
        /// <param name="extra">扩展数据</param>
        public static void LogLevelStart(int levelId, string startType = Consts.EventLevelStartModePlay, 
            string levelType = Consts.LevelTypeMain, string levelName = "", string itemId = "",
            bool isReplay = false, Dictionary<string, object> extra = null)
        {
            if (!IsInitialSuccess)
            {
                LogE($"{LOG_TAG} :: LogLevelStart {levelId} :: Please call <GuruSDK.Start()> first, before you call <LogLevelStart>.");
                return;
            }

            Analytics.LogLevelStart(levelId, levelName, levelType, itemId, startType, isReplay, extra);
        }

        /// <summary>
        /// 游戏点击 Continue 继续游戏 (level_start) (continue)
        /// </summary>
        /// <param name="levelId"></param>
        /// <param name="levelType"></param>
        /// <param name="levelName"></param>
        /// <param name="itemId"></param>
        /// <param name="extra"></param>
        public static void LogLevelContinue(int levelId, string levelType = Consts.LevelTypeMain,
            string levelName = "", string itemId = "", Dictionary<string, object> extra = null)
        {
            LogLevelStart(levelId, Consts.EventLevelStartModeContinue, levelType, levelName, itemId,  true, extra:extra);
        }
        
        /// <summary>
        /// 游戏点击 Replay 重玩关卡 (level_start) (replay)
        /// </summary>
        /// <param name="levelId"></param>
        /// <param name="levelType"></param>
        /// <param name="levelName"></param>
        /// <param name="itemId"></param>
        public static void LogLevelReplay(int levelId, string levelType = Consts.LevelTypeMain,
            string levelName = "", string itemId = "", Dictionary<string, object> extra = null)
        {
            LogLevelStart(levelId, Consts.EventLevelStartModeReplay,levelType, levelName, itemId,  true, extra:extra);
        }

        /// <summary>
        /// 关卡结束打点 (level_end)
        /// </summary>
        /// <param name="levelId">关卡Id</param>
        /// <param name="result">success:成功；fail:失败；exit:退出；timeout:超时；replay:重玩...</param>
        /// <param name="levelType">关卡类型: 主线:main</param>
        /// <param name="levelName">关卡名称: main_01_9001,  daily_challenge_81011</param>
        /// <param name="itemId">配置/谜题/图片/自定义Id: 101120</param>
        /// <param name="duration">关卡完成时长(单位:毫秒)</param>
        /// <param name="step">步数(有则上报)</param>
        /// <param name="score">分数(有则上报)</param>
        /// <param name="extra">扩展数据</param>
        public static void LogLevelEnd(int levelId, string result = Consts.EventLevelEndSuccess,
            string levelType = Consts.LevelTypeMain, string levelName = "", string itemId = "",
            int duration = 0, int? step = null, int? score = null, Dictionary<string, object> extra = null)
        {
            if (!IsInitialSuccess)
            {
                LogE(
                    $"{LOG_TAG} :: LogLevelEnd {levelId} :: Please call <GuruSDK.Start()> first, before you call <LogLevelEnd>.");
                return;
            }
            
            if (extra == null) extra = new Dictionary<string, object>();
            
            // 优先打 level_end 事件
            Analytics.LogLevelEnd(levelId, result, levelName, levelType, itemId, duration, step, score, extra);
            
            // 自动记录关卡属性
            // if (InitConfig.AutoRecordFinishedLevels)
            // {
            //     if (result == Consts.EventLevelEndSuccess)
            //     {
            //         if (levelType.ToLower() == Consts.LevelTypeMain)
            //         {
            //             Model.BLevel = levelId; // 自动记录 [主线] 关卡完成次数
            //         }
            //
            //         Model.BPlay++; // 自动记录关卡总次数
            //
            //         var eventData = new LevelEndSuccessEvent(Model.BPlay, extra);
            //         Analytics.TrackLevelEndSuccessEvent(eventData); // 自动 level_end_success
            //     }
            // }
           
        }

        /// <summary>
        /// 关卡首次通关
        /// </summary>
        /// <param name="levelType"></param>
        /// <param name="levelName"></param>
        /// <param name="level"></param>
        /// <param name="result"></param>
        /// <param name="duration"></param>
        /// <param name="extra"></param>
        public static void LogLevelFirstEnd(string levelType, string levelName, int level,
            string result = Consts.EventLevelEndSuccess, int duration = 0, Dictionary<string, object> extra = null)
        {
            Analytics.LevelFirstEnd(levelType, levelName, level, result, duration, extra);
        }

        /// <summary>
        /// 关卡总胜利次数打点 (level_end_success_{num})
        /// </summary>
        /// <param name="bPlay">完成总关数累计值</param>
        /// /// <param name="extra">扩展参数</param>
        [Obsolete("此方法即将被废弃，请项目组自行实现 level_end_success_{num} 事件打点，中台不再维护此事件和点位")]
        public static void LogLevelEndSuccess(int bPlay, Dictionary<string, object> extra = null)
        {
            var evt = new LevelEndSuccessEvent(bPlay, extra);
            Analytics.TrackLevelEndSuccessEvent(evt);
        }

        /// <summary>
        /// 游戏失败打点 (level_end) (fail)
        /// </summary>
        /// <param name="levelId"></param>
        /// <param name="levelType"></param>
        /// <param name="levelName"></param>
        /// <param name="itemId"></param>
        /// <param name="duration"></param>
        /// <param name="step"></param>
        /// <param name="score"></param>
        /// <param name="extra"></param>
        public static void LogLevelFail(int levelId,
            string levelType = Consts.LevelTypeMain, string levelName = "", string itemId = "",
            int duration = 0, int? step = null, int? score = null , Dictionary<string, object> extra = null)
        {
            LogLevelEnd(levelId, Consts.EventLevelEndFail, levelType, levelName, itemId, duration, step, score, extra);
        }

        /// <summary>
        /// 游戏失败退出 (level_end) (exit) 
        /// </summary>
        /// <param name="levelId"></param>
        /// <param name="levelType"></param>
        /// <param name="levelName"></param>
        /// <param name="itemId"></param>
        /// <param name="duration"></param>
        /// <param name="step"></param>
        /// <param name="score"></param>
        /// <param name="extra"></param>
        public static void LogLevelFailExit(int levelId,
            string levelType = Consts.LevelTypeMain, string levelName = "", string itemId = "",
            int duration = 0, int? step = null, int? score = null, Dictionary<string, object> extra = null)
        {
            LogLevelEnd(levelId, Consts.EventLevelEndExit, levelType, levelName, itemId, duration, step, score, extra);
        }

        /// <summary>
        /// 关卡超时失败 (level_end) (timeout) 
        /// </summary>
        /// <param name="levelId"></param>
        /// <param name="levelType"></param>
        /// <param name="levelName"></param>
        /// <param name="itemId"></param>
        /// <param name="duration"></param>
        /// <param name="step"></param>
        /// <param name="score"></param>
        /// <param name="extra"></param>
        public static void LogLevelFailTimeout(int levelId,
            string levelType = Consts.LevelTypeMain, string levelName = "", string itemId = "",
            int duration = 0, int? step = null, int? score = null, Dictionary<string, object> extra = null)
        {
            LogLevelEnd(levelId, Consts.EventLevelEndTimeout, levelType, levelName, itemId, duration, step, score, extra);
        }


        /// <summary>
        /// 玩家(角色)升级事件 (level_up)
        /// </summary>
        /// <param name="playerLevel"></param>
        /// <param name="characterName"></param>
        /// <param name="extra">扩展数据</param>
        public static void LogLevelUp(int playerLevel, string characterName, Dictionary<string, object> extra = null)
        {
            if (!IsInitialSuccess)
            {
                LogE($"{LOG_TAG} :: LogLevelUp {playerLevel} :: Please call <GuruSDK.Start()> first, before you call <LogLevelUp>.");
                return;
            }
            Analytics.LevelUp(playerLevel, characterName, extra);
        }

        /// <summary>
        /// 玩家解锁成就 (unlock_achievement)
        /// </summary>
        /// <param name="achievementId"></param>
        /// <param name="extra">扩展数据</param>
        public static void LogAchievement(string achievementId, Dictionary<string, object> extra = null)
        {
            if (!IsInitialSuccess)
            {
                LogE($"{LOG_TAG} :: LogAchievement {achievementId} :: Please call <GuruSDK.Start()> first, before you call <LogAchievement>.");
                return;
            }
            Analytics.UnlockAchievement(achievementId, extra);
        }

        /// <summary>
        /// 玩家体力变化 (hp_points)
        /// </summary>
        /// <param name="scene">场景</param>
        /// <param name="hp">HP 增量值</param>
        /// <param name="hpBefore">HP 初始值</param>
        /// <param name="hpAfter">HP 结算值</param>
        /// <param name="extra">额外数据</param>
        public static void LogHpPoints(string scene, int hp, int hpBefore, int hpAfter,
            Dictionary<string, object> extra = null)
        {
            if (!IsInitialSuccess)
            {
                UnityEngine.Debug.LogError(
                    $"{LOG_TAG} :: LogHpPoints {hp} :: Please call <GuruSDK.Start()> first, before you call <LogHpChanged>.");
                return;
            }

            var dict = new Dictionary<string, object>()
            {
                [Consts.ParameterItemCategory] = scene,
                ["hp"] = hp,
                ["hp_before"] = hpBefore,
                ["hp_after"] = hpAfter,
            };

            if (extra != null)
            {
                foreach (var k in extra.Keys)
                {
                    dict[k] = extra[k];
                }
            }

            LogEvent(Consts.EventHpPoints, dict);
        }

        #endregion
        
        #region SDK 打点

        /// <summary>
        /// 获取 GuruSDK 实验分组
        /// </summary>
        /// <returns></returns>
        public static string GetGuruExperimentGroupId()
        {
            if (!GuruAnalytics.IsReady) return "not_set";
            return GuruAnalytics.Instance.ExperimentGroupId;
        }

        #endregion

        #region IAP 打点

        private static string TryGetFirstProductId()
        {
            if (GuruSettings.Instance != null && (GuruSettings.Instance.Products?.Length ?? 0) > 0)
            {
                return GuruSettings.Instance.Products[0]?.ProductId ?? "";
            }

            return "";
        }
        
        private static string TryGetCurrentProductId()
        {
            if (GuruIAP.Instance != null && IsIAPReady)
            {
                return GuruIAP.Instance.CurrentBuyingProductId;
            }
            return "";
        }
        
        

        /// <summary>
        /// 当付费页面打开时调用 (iap_imp)
        /// </summary>
        /// <param name="scene">付费页场景名称</param>
        /// <param name="extra"></param>
        public static void LogIAPImp(string scene, Dictionary<string, object> extra = null)
        {
            Analytics.IAPImp(scene, extra);
        }

        /// <summary>
        /// 当付费页面关闭时调用 (iap_close)
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="extra"></param>
        public static void LogIAPClose(string scene, Dictionary<string, object> extra = null)
        {
            Analytics.IAPClose(scene, extra);
        }

        /// <summary>
        /// 当点击 IAP 商品按钮的时候调用 (iap_clk)
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="productId"></param>
        /// <param name="basePlan"></param>
        /// <param name="offerId"></param>
        /// <param name="extra"></param>
        public static void LogIAPClick(string scene, string productId, string basePlan = "", string offerId = "", Dictionary<string, object> extra = null)
        {
            Analytics.IAPClick(scene, productId, basePlan, offerId, extra);
        }
        
        #endregion

        #region 经济打点
        
        // ************************************************************************************************
        // *
        // *                                        经济打点
        // * 内容详参: https://docs.google.com/spreadsheets/d/1xYSsAjbrwqeJm7panoVzHO0PeGRQVDCR4e2CU9OPEzk/edit#gid=0
        // *
        // ************************************************************************************************
        
        //---------------------------------------- EARN ---------------------------------------- 
        /// <summary>
        /// 基础收入接口 (earn_virtual_currency)
        /// 可直接调用此接口上报相关参数
        /// 获取虚拟货币/道具. 
        /// 基础接口, 不推荐项目组直接调用
        /// 请直接调用其他对应场景的统计接口
        /// </summary>
        /// <param name="currencyName">货币名称</param>
        /// <param name="value">货币增加值 10</param>
        /// <param name="balance">结算后货币总量 20 -> 30</param>
        /// <param name="category">消耗类型, 默认值请赋 reward</param>
        /// <param name="levelName">当前关卡或者人物等级名称</param>
        /// <param name="itemName">购买道具名称</param>
        /// <param name="scene">购买场景如 Store, Workbench, Sign, Ads....</param>
        /// <param name="extra">自定义数据</param>
        public static void LogEarnVirtualCurrency(string currencyName, 
            int value, int balance, 
            string category = "", string itemName = "",
            string levelName = "", string scene = "", Dictionary<string, object> extra = null)
        {
            Analytics.EarnVirtualCurrency(currencyName, value, balance, category, itemName,levelName, scene, extra);
        }
        
        /// <summary>
        /// 游戏初次启动/用户获得初始道具
        /// </summary>
        /// <param name="currencyName">货币名称</param>
        /// <param name="value">货币增加值 10</param>
        /// <param name="balance">结算后货币总量 20 -> 30</param>
        /// <param name="levelName">购入道具 ID / 道具名称</param>
        /// <param name="scene">购买场景如 Store, Workbench, Sign, Ads....</param>
        /// <param name="extra">自定义数据</param>
        public static void LogEarnVirtualCurrencyByFirstOpen(string currencyName, 
            int value, int balance, 
            string levelName = "", string scene = "", Dictionary<string, object> extra = null)
        {
            string item_category = Consts.CurrencyCategoryReward;
            string item_name = "first_open";
            Analytics.EarnVirtualCurrency(currencyName, value, balance, item_category, item_name,levelName, scene, extra);
        }

        /// <summary>
        /// 出售道具后获取货币
        /// </summary>
        /// <param name="currencyName">货币名称</param>
        /// <param name="value">货币增加值 10</param>
        /// <param name="balance">结算后货币总量 20 -> 30</param>
        /// <param name="itemName">购买道具名称</param>
        /// <param name="levelName">当前关卡或者人物等级名称</param>
        /// <param name="scene">购买场景如 Store, Workbench, Sign, Ads....</param>
        /// <param name="extra">自定义数据</param>
        public static void LogEarnVirtualCurrencyBySellItem(string currencyName, 
            int value, int balance, string itemName,
            string levelName = "", string scene = "", Dictionary<string, object> extra = null)
        {
            string item_category = Consts.CurrencyCategoryIGC;
            Analytics.EarnVirtualCurrency(currencyName, value, balance, item_category, itemName,levelName, scene, extra);
        }
        
        
        /// <summary>
        /// 赚取组合: 货币+道具 (earn_virtual_currency) (props)
        /// </summary>
        /// <param name="currencyName">货币名称</param>
        /// <param name="value">货币增加值 10</param>
        /// <param name="balance">结算后货币总量 20 -> 30</param>
        /// <param name="category">消耗类型, 默认值请赋 reward</param>
        /// <param name="itemName">购买道具名称</param>
        /// <param name="props">获取的道具组合</param>
        /// <param name="levelName">当前关卡或者人物等级名称</param>
        /// <param name="scene">购买场景如 Store, Workbench, Sign, Ads....</param>
        /// <param name="extra">自定义数据</param>
        private static void LogEarnVirtualCurrencyAndProps(string currencyName, 
            int value = 0, int balance = 0,
            string category = "", string itemName = "",
            string levelName = "", string scene = Consts.ParameterDefaultScene,  
            string[] props = null, Dictionary<string, object> extra = null)
        {
            //---- Currency -----
            if (value > 0)
            {
                LogEarnVirtualCurrency(currencyName, value, balance, category, itemName, levelName, scene, extra);
            }
            //---- Props --------
            if (null != props)
            {
                int i = 0;
                while (i < props.Length)
                {
                    LogEarnVirtualCurrency(props[i], 1, 0, category, itemName, levelName, scene, extra);
                    i++;
                }
            }
        }

        /// <summary>
        /// 签到奖励. 获得货币/道具 (earn_virtual_currency) (reward:sign)
        /// <li>通常类型: Coin 收入 </li>
        /// <li>特殊类型: Coin + Props (道具列表) </li>
        /// <li>特殊类型: Props (道具列表) </li>
        /// </summary>
        /// <param name="currencyName">货币名称</param>
        /// <param name="value">赚取金额</param>
        /// <param name="balance">货币总量(累加后)</param>
        /// <param name="levelName">当前关卡名称</param>
        /// <param name="scene">应用场景</param>
        /// <param name="props">获取的道具名称列表</param>
        /// <param name="extra"></param>
        public static void LogEarnVirtualCurrencyBySign(string currencyName, 
            int value = 0, int balance = 0, string levelName = "", 
            string scene = "home_page", string[] props = null, Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryReward;
            string itemName = "sign";

            LogEarnVirtualCurrencyAndProps(currencyName, value, balance, category, itemName, levelName, scene, props, extra);
        }


        /// <summary>
        /// IAP 付费购买. 获得货币/道具 (earn_virtual_currency) (iap_buy:sku)
        /// <li>通常类型: Coin 收入 </li>
        /// <li>特殊类型: Coin + Props (道具列表) </li>
        /// <li>特殊类型: Props (道具列表) </li> 
        /// </summary>
        /// <param name="currencyName">IAP 道具名称</param>
        /// <param name="value">赚取金额</param>
        /// <param name="balance">货币总量(累加后)</param>
        /// <param name="levelName">当前关卡名称</param>
        /// <param name="scene">应用场景</param>
        /// <param name="props">获取的道具名称列表</param>
        /// <param name="extra"></param>
        public static void LogEarnVirtualCurrencyByIAP(string currencyName,
            int value = 0, int balance = 0, string levelName = "", 
            string scene = "store", string[] props = null, Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryIAP;
            // string itemName = productId;
            string itemName = "sku";
            LogEarnVirtualCurrencyAndProps(currencyName, value, balance, category, itemName, levelName, scene, props, extra);
        }

        /// <summary>
        /// 看广告获取到货币/道具 (earn_virtual_currency) (reward:ads)
        /// <li>通常类型: Coin 收入 </li>
        /// <li>特殊类型: Coin + Props (道具列表) </li>
        /// <li>特殊类型: Props (道具列表) </li> 
        /// </summary>
        /// <param name="currencyName">货币名称</param>
        /// <param name="value">赚取金额</param>
        /// <param name="balance">货币总量(累加后)</param>
        /// <param name="levelName">当前关卡名称</param>
        /// <param name="scene">应用场景</param>
        /// <param name="props">获取的道具名称列表</param>
        /// <param name="extra"></param>
        public static void LogEarnVirtualCurrencyByAds(string currencyName, 
            int value = 0, int balance = 0, string levelName = "", 
            string scene = "store", string[] props = null, Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryReward;
            string itemName = "ads";
            LogEarnVirtualCurrencyAndProps(currencyName, value, balance, category, itemName, levelName, scene, props, extra);
        }

        /// <summary>
        /// 使用了金币半价 + 看广告获取到货币/道具 (earn_virtual_currency) (bonus:ads)
        /// <li>通常类型: Coin 收入 </li>
        /// <li>特殊类型: Coin + Props (道具列表) </li>
        /// <li>特殊类型: Props (道具列表) </li> 
        /// </summary>
        /// <param name="currencyName">货币名称</param>
        /// <param name="value">赚取金额</param>
        /// <param name="balance">货币总量(累加后)</param>
        /// <param name="levelName">当前关卡名称</param>
        /// <param name="scene">应用场景</param>
        /// <param name="props">获取的道具名称列表</param>
        /// <param name="extra"></param>
        public static void LogEarnVirtualCurrencyByPaidAds(string currencyName, 
            int value = 0, int balance = 0, string levelName = "", 
            string scene = Consts.ParameterDefaultScene, string[] props = null, Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryBonus;
            string itemName = "ads";
            LogEarnVirtualCurrencyAndProps(currencyName, value, balance, category, itemName, levelName, scene, props, extra);
        }

        /// <summary>
        /// 过关奖励获取到货币/道具 (earn_virtual_currency) (reward:level) 
        /// <li>通常类型: Coin 收入 </li>
        /// <li>特殊类型: Coin + Props (道具列表) </li>
        /// <li>特殊类型: Props (道具列表) </li> 
        /// </summary>
        /// <param name="currencyName">货币名称</param>
        /// <param name="value">赚取金额</param>
        /// <param name="balance">货币总量(累加后)</param>
        /// <param name="levelName">当前关卡名称</param>
        /// <param name="scene">应用场景</param>
        /// <param name="props">获取的道具名称列表</param>
        /// <param name="extra"></param>
        public static void LogEarnVirtualCurrencyByLevelComplete(string currencyName, 
            int value = 0, int balance = 0, string levelName = "", 
            string scene = "store", string[] props = null, Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryReward;
            string itemName = "level";
            LogEarnVirtualCurrencyAndProps(currencyName, value, balance, category, itemName, levelName, scene, props, extra);
        }


        /// <summary>
        /// 购买获得 Prop (earn_virtual_currency) (reward:level) 
        /// 记录 Prop 增加的打点, 消费游戏内货币
        /// </summary>
        /// <param name="currencyName">购买的道具名称</param>
        /// <param name="spendCurrencyName">消费货币名称</param>
        /// <param name="scene">应用场景</param>
        /// <param name="value">购入数量</param>
        /// <param name="balance">道具总量(累加后)</param>
        /// <param name="levelName">当前关卡名称</param>
        /// <param name="extra"></param>
        public static void LogEarnVirtualCurrencyByIGC(string currencyName, string spendCurrencyName, int value = 0, int balance = 0, 
            string levelName = "", string scene = "", Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryIGC;
            string itemName = spendCurrencyName;
            if (string.IsNullOrEmpty(scene)) scene = Consts.ParameterDefaultScene;
            LogEarnVirtualCurrencyAndProps(currencyName, value, balance, category, itemName, levelName, scene, null, extra); // TODO 这里的打点不对
        }

        /// <summary>
        /// 通过道具交换/合成或得了其他道具 (earn_virtual_currency) (igb:coin) 
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="otherName"></param>
        /// <param name="scene"></param>
        /// <param name="value"></param>
        /// <param name="balance"></param>
        /// <param name="levelName"></param>
        /// <param name="extra"></param>
        public static void LogEarnPropByProp(string propName, string otherName,
            string scene = Consts.ParameterDefaultScene,
            int value = 1, int balance = 0, string levelName = "", Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryIGB;
            LogEarnVirtualCurrency(propName, value, balance, category, otherName, levelName, scene, extra);
        }


        /// <summary>
        /// 通过转盘或者抽奖, 获取货币/道具 (earn_virtual_currency) (igb:lottery) 
        /// <li>通常类型: Coin 收入 </li>
        /// <li>特殊类型: Coin + Props (道具列表) </li>
        /// <li>特殊类型: Props (道具列表) </li> 
        /// </summary>
        /// <param name="currencyName">货币名称</param>
        /// <param name="value">赚取金额</param>
        /// <param name="balance">货币总量(累加后)</param>
        /// <param name="levelName">当前关卡名称</param>
        /// <param name="scene">应用场景</param>
        /// <param name="props">获取的道具名称列表</param>
        /// <param name="extra"></param>
        public static void LogEarnVirtualCurrencyByLottery(string currencyName, 
            int value = 0, int balance = 0, string levelName = "", 
            string scene = "store", string[] props = null, Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryIGB;
            string itemName = "lottery";
            LogEarnVirtualCurrencyAndProps(currencyName, value, balance, category, itemName, levelName, scene, props, extra);
        }
        
        
        //---------------------------------------- SPEND ---------------------------------------- 


        /// <summary>
        /// 基础花费虚拟货币/道具 (spend_virtual_currency)
        /// 基础接口, 不推荐项目组直接调用
        /// 请直接调用其他对应场景的统计接口
        /// </summary>
        /// <param name="currencyName">货币名称</param>
        /// <param name="value">货币消耗值 10</param>
        /// <param name="balance">结算后货币总量 30 -> 20</param>
        /// <param name="category">消耗类型, 默认值请赋 reward</param>
        /// <param name="levelName">当前关卡或者人物等级名称</param>
        /// <param name="itemName">购买道具名称</param>
        /// <param name="scene">购买场景如 Store, Workbench, Sign, Ads....</param>
        /// <param name="extra"></param>
        public static void LogSpendVirtualCurrency(string currencyName, int value, int balance, string category = "", string itemName = "",
            string levelName = "", string scene = "", Dictionary<string, object> extra = null)
        {
            Analytics.SpendVirtualCurrency(currencyName, value, balance, category, itemName, levelName, scene, extra);
        }
        
        /// <summary>
        /// 消耗 Boost 道具
        /// </summary>
        /// <param name="currencyName">货币/道具名称</param>
        /// <param name="value">货币消耗值 10</param>
        /// <param name="balance">结算后货币总量 30 -> 20</param>
        /// <param name="itemName"></param>
        /// <param name="levelName"></param>
        /// <param name="scene"></param>
        /// <param name="extra"></param>
        public static void LogSpendVirtualCurrencyAsBoost(string currencyName, int value, int balance, string itemName = "",
            string levelName = "", string scene = "", Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryBoost;
            Analytics.SpendVirtualCurrency(currencyName, value, balance, category, itemName, levelName, scene, extra);
        }
        
        /// <summary>
        /// 消耗货币购买道具 (spend_virtual_currency) (props)
        /// </summary>
        /// <param name="currencyName"></param>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        /// <param name="balance"></param>
        /// <param name="levelName"></param>
        /// <param name="scene"></param>
        /// <param name="extra"></param>
        public static void LogSpendVirtualCurrencyWithProp(string currencyName, string prop,
            int value, int balance,
            string levelName = "", string scene = "", Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryProp;
            LogSpendVirtualCurrency(currencyName, value, balance, category, prop, levelName, scene, extra); 
        }

        /// <summary>
        /// 消耗货币购买道具 (含多个) (spend_virtual_currency) (props)
        /// </summary>
        /// <param name="currencyName"></param>
        /// <param name="props"></param>
        /// <param name="value"></param>
        /// <param name="balance"></param>
        /// <param name="levelName"></param>
        /// <param name="scene"></param>
        /// <param name="extra"></param>
        public static void LogSpendVirtualCurrencyWithProps(string currencyName, string[] props,
            int value, int balance, 
            string levelName = "", string scene = "", Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryProps;
            if (props != null && props.Length > 0)
            {
                int i = 0;
                while (i < props.Length)
                {
                    LogSpendVirtualCurrency(currencyName, value, balance, category, props[i], levelName, scene, extra); 
                    i++;
                }
            }
        }

        /// <summary>
        /// 消耗货币购买礼包或组合 (spend_virtual_currency) (bundle)
        /// </summary>
        /// <param name="currencyName"></param>
        /// <param name="bundle"></param>
        /// <param name="value"></param>
        /// <param name="balance"></param>
        /// <param name="levelName"></param>
        /// <param name="scene"></param>
        /// <param name="extra"></param>
        public static void LogSpendVirtualCurrencyWithBundle(string currencyName, string bundle,
            int value, int balance,
            string levelName = "", string scene = "", Dictionary<string, object> extra = null)
        {
            LogSpendVirtualCurrencyWithBundles(currencyName, new string[] {bundle}, value, balance, levelName, scene, extra);
        }

        /// <summary>
        /// 消耗货币购买礼包或组合 (复数) (spend_virtual_currency) (bundle)
        /// </summary>
        /// <param name="currencyName"></param>
        /// <param name="bundles"></param>
        /// <param name="value"></param>
        /// <param name="balance"></param>
        /// <param name="levelName"></param>
        /// <param name="scene"></param>
        /// <param name="extra"></param>
        public static void LogSpendVirtualCurrencyWithBundles(string currencyName, string[] bundles,
            int value, int balance, 
            string levelName = "", string scene = "", Dictionary<string, object> extra = null)
        {
            string category = Consts.CurrencyCategoryBundle;
            if (bundles != null && bundles.Length > 0)
            {
                int i = 0;
                while (i < bundles.Length)
                {
                    LogSpendVirtualCurrency(currencyName, value, balance, category, bundles[i], levelName, scene, extra); 
                    i++;
                }
            }
        }

        /// <summary>
        /// 消耗物品, 交换其他物品 (spend_virtual_currency) (prop)
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="otherName"></param>
        /// <param name="value"></param>
        /// <param name="balance"></param>
        /// <param name="levelName"></param>
        /// <param name="scene"></param>
        /// <param name="extraCategory"></param>
        /// <param name="extra"></param>
        public static void LogSpendPropWithProp(string propName, string otherName,
            int value, int balance, 
            string levelName = "", string scene = "", string extraCategory = "", Dictionary<string, object> extra = null)
        {
            string category = string.IsNullOrEmpty(extraCategory) ? Consts.CurrencyCategoryProp : extraCategory;
            LogSpendVirtualCurrency(propName, value, balance, category, otherName, levelName, scene, extra); 
        }


        #endregion

        #region ATT 二次引导弹窗
        
        /// <summary>
        /// ATT 二次引导弹窗展示 (att_reguide_imp)
        /// </summary>
        /// <param name="type">引导样式名称</param>
        /// <param name="showTimes">第几次展示</param>
        /// <param name="installDays">举例安装日期的天数</param>
        /// <param name="extra">扩展数据</param>
        public static void LogAttReguideImp(string type, 
            int showTimes = 1, int installDays = 0, Dictionary<string, object> extra = null)
        {
            var dict = new Dictionary<string, object>()
            {
                ["type"] = type,
                ["show_times"] = showTimes,
                ["install_days"] = installDays,
            };

            if (extra != null) dict = GuruSDKUtils.MergeDictionary(dict, extra);
            LogEvent(Consts.EventAttReguideImp, dict);
        }
        
        /// <summary>
        /// ATT 二次引导弹窗点击 (att_reguide_clk)
        /// </summary>
        /// <param name="type">引导样式名称</param>
        /// <param name="showTimes">第几次展示</param>
        /// <param name="installDays">举例安装日期的天数</param>
        /// <param name="action">点击引导页的动作：dissmiss/go</param>
        /// <param name="extra">扩展数据</param>
        public static void LogAttReguideClick(string type, string action, 
            int showTimes = 1, int installDays = 0, Dictionary<string, object> extra = null)
        {
            var dict = new Dictionary<string, object>()
            {
                ["type"] = type,
                ["show_times"] = showTimes,
                ["install_days"] = installDays,
                ["action"] = action,
            };

            if (extra != null) dict = GuruSDKUtils.MergeDictionary(dict, extra);
            LogEvent(Consts.EventAttReguideClk, dict);
        }

        /// <summary>
        /// ATT 二次引导弹窗展示结果 (att_reguide_result)
        /// </summary>
        /// <param name="type">引导样式名称</param>
        /// <param name="showTimes">第几次展示</param>
        /// <param name="installDays">举例安装日期的天数</param>
        /// <param name="action">点击引导页的动作：dissmiss/go</param>
        /// <param name="result">结果字段：authorized, denied, restricted, notDetermined</param>
        /// <param name="extra">扩展数据</param>
        public static void LogAttReguideResult(string type, string action, string result, 
            int showTimes = 1, int installDays = 0, Dictionary<string, object> extra = null)
        {
            var dict = new Dictionary<string, object>()
            {
                ["type"] = type,
                ["show_times"] = showTimes,
                ["install_days"] = installDays,
                ["action"] = action,
                [Consts.ParameterItemCategory] = result,
            };

            if (extra != null) dict = GuruSDKUtils.MergeDictionary(dict, extra);
            LogEvent(Consts.EventAttReguideResult, dict);
        }
        

        #endregion
        
        #region 教程引导
        
        /// <summary>
        /// 教程开始
        /// </summary>
        /// <param name="extra">扩展数据</param>
        public static void LogTutorialBegin(Dictionary<string, object> extra = null)
        {
            LogEvent(Consts.EventTutorialBegin, extra);
        }

        /// <summary>
        /// 教程开始
        /// </summary>
        /// <param name="step">教程步骤</param>
        /// <param name="extra">扩展数据</param>
        public static void LogTutorialImp(int step, Dictionary<string, object> extra = null)
        {
            string key = Consts.EventTutorialImp.Replace("{0}", step.ToString());
            LogEvent(key, extra);
        }

        /// <summary>
        /// 教程开始
        /// </summary>
        /// <param name="step">教程步骤</param>
        /// <param name="extra">扩展数据</param>
        public static void LogTutorialNextClick(int step, Dictionary<string, object> extra = null)
        {
            string key = Consts.EventTutorialNextClick.Replace("{0}", step.ToString());
            LogEvent(key, extra);
        }

        /// <summary>
        /// 教程结束
        /// </summary>
        /// <param name="extra">扩展数据</param>
        public static void LogTutorialComplete(Dictionary<string, object> extra = null)
        {
            LogEvent(Consts.EventTutorialComplete, extra);
        }

        /// <summary>
        /// 教程页面关闭
        /// </summary>
        /// <param name="extra">扩展数据</param>
        public static void LogTutorialClose(Dictionary<string, object> extra = null)
        {
            LogEvent(Consts.EventTutorialClose, extra);
        }
        #endregion

        #region 消息权限弹窗打点

        /// <summary>
        /// 通知栏请求权限展示时触发 (noti_perm_imp)
        /// </summary>
        /// <param name="style">弹窗样式</param>
        /// <param name="requestTimes">请求弹窗的次数</param>
        /// <param name="showTimes">展现弹窗的次数</param>
        /// <param name="deniedTimes">点击 deny 的次数</param>
        /// <param name="promptTrigger">弹窗触发来源</param>
        /// <param name="scene">弹窗场景</param>
        /// <param name="extra">扩展参数</param>
        public static void LogNotiPermImp(int requestTimes,  int showTimes, int deniedTimes, string promptTrigger,string style = "default",  string scene = "", Dictionary<string, object> extra = null)
        {
            var dict = new Dictionary<string, object>()
            {
                [Consts.ParameterItemCategory] = style,
                ["request_times"] = requestTimes,
                ["show_times"] = showTimes,
                ["denied_times"] = deniedTimes,
                ["prompt_trigger"] = promptTrigger,
            };

            if (!string.IsNullOrEmpty(scene))
                dict[Consts.ParameterItemName] = scene;

            if (extra != null) dict = GuruSDKUtils.MergeDictionary(dict, extra);
            LogEvent(Consts.EventNotiPermImp, dict);
        }
        
        /// <summary>
        /// 得到权限结果时触发 (noti_perm_result)
        /// </summary>
        /// <param name="requestTimes"></param>
        /// <param name="showTimes"></param>
        /// <param name="deniedTimes"></param>
        /// <param name="result"></param>
        /// <param name="promptTrigger"></param>
        /// <param name="style"></param>
        /// <param name="scene"></param>
        /// <param name="extra"></param>
        public static void LogNotiPermResult( int requestTimes,  int showTimes, int deniedTimes, string result, string promptTrigger, string style = "default", string scene = "", Dictionary<string, object> extra = null)
        {
            var dict = new Dictionary<string, object>()
            {
                ["request_times"] = requestTimes,
                ["show_times"] = showTimes,
                ["denied_times"] = deniedTimes,
                ["prompt_trigger"] = promptTrigger,
                ["result"] = result,
                [Consts.ParameterItemCategory] = style,
            };

            if (!string.IsNullOrEmpty(scene))
                dict[Consts.ParameterItemName] = scene;

            if (extra != null) dict = GuruSDKUtils.MergeDictionary(dict, extra);
            LogEvent(Consts.EventNotiPermResult, dict);
        }
        
        /// <summary>
        /// 说明性通知栏权限引导展示时触发 (noti_perm_rationale_imp)
        /// </summary>
        /// <param name="pageName">引导页的名称，自定义</param>
        /// <param name="scene"></param>
        /// <param name="extra"></param>
        public static void LogNotiPermRationaleImp(string pageName, string scene = "", Dictionary<string, object> extra = null)
        {
            var dict = new Dictionary<string, object>()
            {
                [Consts.ParameterItemCategory] = pageName,
            };

            if (!string.IsNullOrEmpty(scene))
                dict[Consts.ParameterItemName] = scene;

            if (extra != null) dict = GuruSDKUtils.MergeDictionary(dict, extra);
            LogEvent(Consts.EventNotiPermRationaleImp, dict);
        }
        
        /// <summary>
        /// 说明性通知栏权限引导结果时触发 (noti_perm_rationale_result)
        /// </summary>
        /// <param name="result"></param>
        /// <param name="pageName"></param>
        /// <param name="scene"></param>
        /// <param name="extra"></param>
        public static void LogNotiPermRationaleResult(string result, string pageName, string scene = "", Dictionary<string, object> extra = null)
        {
            var dict = new Dictionary<string, object>()
            {
                ["result"] = result,
                [Consts.ParameterItemCategory] = pageName,
            };

            if (!string.IsNullOrEmpty(scene))
                dict[Consts.ParameterItemName] = scene;

            if (extra != null) dict = GuruSDKUtils.MergeDictionary(dict, extra);
            LogEvent(Consts.EventNotiPermRationaleResult, dict);
        }

        #endregion

        #region 错误时间上报
        
        /// <summary>
        /// 上报 dev_audit 异常事件
        /// </summary>
        /// <param name="dict"></param>
        public static void LogDevAudit(Dictionary<string, object> dict)
        {
            Analytics.LogDevAudit(dict);
        }


        #endregion
        
        #region Crashlytics 接口

        public static void CrashLog(string message)
        {
            if (!IsFirebaseReady) return;
            CrashlyticsAgent.Log(message);
        }
        
        public static void CrashLogException(string message)
        {
            if (!IsFirebaseReady) return;
            CrashlyticsAgent.LogException(message);
        }

        public static void CrashException(Exception ex)
        {
            if (!IsFirebaseReady) return;
            CrashlyticsAgent.LogException(ex);
        }

        public static void CrashSetCustomKeys(string key, string value)
        {
            if (!IsFirebaseReady) return;
            CrashlyticsAgent.SetCustomKey(key, value);
        }
        #endregion

        #region 优先级设置
        
        private static readonly Dictionary<string, int> _eventPriorities = new Dictionary<string, int>(10);
        
        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="eventNames"></param>
        public static void SetEventPriority(EventPriority priority, string[] eventNames)
        {
            int i = 0;
            while (i < eventNames.Length)
            {
                var evt = eventNames[i];
                if (!string.IsNullOrEmpty(evt))
                {
                    _eventPriorities[evt] = (int)priority;
                }
                i++;
            }
        }

        public static void SetEventPriority(EventPriority priority, string eventName)
        {
            SetEventPriority(priority, new string[]{eventName});
        }

        public static EventPriority GetEventPriority(string eventName)
        {
            if (_eventPriorities.TryGetValue(eventName, out int p))
            {
                return (EventPriority)p;
            }
            return EventPriority.Default;
        }

        public static int GetEventPriorityInt(string eventName)
        {
            return (int)GetEventPriority(eventName);
        }

        /// <summary>
        /// set all events as 'Emergence' event, which will be triggered immediately
        /// </summary>
        private void SetSDKEventPriority()
        {
            SetEventPriority(EventPriority.Emergence, new []
            {
                Consts.EventTchAdRev001Impression,
                Consts.EventTchAdRev02Impression,
                Consts.EventLevelStart,
                Consts.EventLevelEnd,
                Consts.EventIAPReturnTrue,
                Consts.EventIAPPurchase,
            });
        }

        #endregion
    }


}