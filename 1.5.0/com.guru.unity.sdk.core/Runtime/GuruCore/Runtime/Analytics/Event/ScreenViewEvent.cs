
namespace Guru
{
    using System.Collections.Generic;

    public class ScreenViewEvent: ITrackingEvent
    {
        public string EventName { get; }
        public EventPriority Priority { get; }
        public Dictionary<string, object> Data { get; }
        public EventSetting Setting{ get; }

        public ScreenViewEvent(string screenName, string className = "")
        {
            EventName = Analytics.EventScreenView;
            Priority = EventPriority.Default;
            Setting = new EventSetting()
            {
                EnableFirebaseAnalytics = true,
                EnableGuruAnalytics = false,
                EnableAdjustAnalytics = false,
                EnableFacebookAnalytics = false
            };
            
            Data = new Dictionary<string, object>()
            {
                [Analytics.ParameterScreenName] = screenName,
            };
            if (!string.IsNullOrEmpty(className))
            {
                Data[Analytics.ParameterScreenClass] = className;
            }
        }
    }
}