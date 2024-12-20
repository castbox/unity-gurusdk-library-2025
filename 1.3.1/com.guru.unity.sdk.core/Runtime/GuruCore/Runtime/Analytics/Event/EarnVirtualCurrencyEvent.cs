
namespace Guru
{
    using System.Collections.Generic;

    /// <summary>
    /// 道具收益事件
    /// </summary>
    public class EarnVirtualCurrencyEvent: ITrackingEvent, IFBEvent
    {
        public string EventName { get; }
        public Dictionary<string, object> Data { get; }
        public EventSetting Setting { get; }
        public EventPriority Priority { get; }
        public float Value { get; }
        
        private readonly string _currencyName;
        private readonly string _category;
        private readonly string _itemName;
        
        public EarnVirtualCurrencyEvent(string currencyName, 
            int value = 1, int balance = 0, 
            string category = "", 
            string itemName = "",
            string levelName = "0",
            string scene = "", Dictionary<string, object> extra = null)
        {
            EventName = Analytics.EventEarnVirtualCurrency;
            Priority = EventPriority.Default;
            Value = value;
            _currencyName = currencyName;
            _category = category;
            _itemName = itemName;
            
            Setting = new EventSetting()
            {
                EnableFacebookAnalytics = true,
                EnableGuruAnalytics = true,
                EnableFirebaseAnalytics = true
            };
            Data = new Dictionary<string, object>()
            {
                { Analytics.ParameterVirtualCurrencyName, currencyName },
                { Analytics.ParameterValue, value },
                { Analytics.ParameterBalance, balance },
                { Analytics.ParameterLevelName, levelName },
                { Analytics.ParameterItemName, itemName }, 
                { Analytics.ParameterItemCategory, category },
                // { Analytics.ParameterScene, scene },
            };
            if (!string.IsNullOrEmpty(scene))
            {
                Data[Analytics.ParameterScene] = scene; // 获取的虚拟货币或者道具的场景    
            }

            // --- 记录 Extra ---
            if (extra == null) return;
            foreach (var key in extra.Keys)
            {
                Data[key] = extra[key];
            }
        }

        public override string ToString()
        {
            return $"{EventName} --- currencyName:{_currencyName}  value:{Value}  category:{_category}  itemName:{_itemName}";
        }
    }
}