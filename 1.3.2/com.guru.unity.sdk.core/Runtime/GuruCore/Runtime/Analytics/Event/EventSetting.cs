namespace Guru
{
    public class EventSetting
    {
        public bool EnableFirebaseAnalytics;
        public bool EnableAdjustAnalytics;
        public bool EnableFacebookAnalytics;
        public bool EnableGuruAnalytics = true; // 默认开启自打点

        public override string ToString()
        {
            return $"EvenSetting: firebase:{EnableFirebaseAnalytics}, adjust:{EnableAdjustAnalytics}, facebook:{EnableFacebookAnalytics}, guru:{EnableGuruAnalytics}";
        }

        public static EventSetting GetFullSetting()
        {
            return new EventSetting()
            {
                EnableFirebaseAnalytics = true,
                EnableGuruAnalytics = true,
                EnableFacebookAnalytics = true,
                EnableAdjustAnalytics = true,
            };
        }
        
        public static EventSetting FirebaseAndGuru()
        {
            return new EventSetting()
            {
                EnableFirebaseAnalytics = true,
                EnableGuruAnalytics = true,
                EnableFacebookAnalytics = false,
                EnableAdjustAnalytics = false,
            };
        }
    }
}