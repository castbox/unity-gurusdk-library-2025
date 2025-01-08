

namespace Guru
{
    using System;
    using UnityEngine;

    public static class GlobalVars
    {
        
        public class Events
        {
            public const string EventTabClicked  = "evt_tab_clicked";
            public const string EventViewClosed  = "evt_view_closed";
            public const string EventMonitorClicked  = "evt_monitor_clicked";

            public static Action<string, object> OnUIEvent = (e, o) => { };
        }
        

        public class Consts
        {
            public const string DefaultTabName = "Tab";
            public const string DefaultOptionName = "Opt";
        }

        public class Colors
        {
            public static Color Gray = new Color(1,1,1, 0.12f);
            public static Color Gray2 = new Color(1,1,1, 0.036f);
            public static Color LightGreen = new Color(0.02f,1,1, 0.788f);
        }

    }
}