
namespace Guru
{
    using System;
    
    [Serializable]
    public class EventConfig
    {
        public string firebaseAppInstanceId; // firebase实例id，用于标识一个用户
        public string idfa; // adjust广告id（ios）
        public string adid; // adjust设备id（ios）
        public string gpsAdid; // adjust广告id
        public string idfv; // 
        public string afid; //AppsflyerId

        public static EventConfig Build()
        {
            var config = new EventConfig()
            {
                firebaseAppInstanceId = IPMConfig.FIREBASE_ID,
                idfa = IPMConfig.IDFA,
                idfv = IPMConfig.IDFV,
                adid = IPMConfig.ADJUST_DEVICE_ID,
                gpsAdid = IPMConfig.GOOGLE_ADID,
                afid = IPMConfig.APPSFLYER_ID,
            };
            
            return config;
        }
        
        /// <summary>
        /// 直接构建 JSON 串
        /// </summary>
        /// <returns></returns>
        public static string BuildJson()
        {
            var config = Build().ToJson();
            return config;
        }
        

        public string ToJson()
        {
            return $"{{\"firebaseAppInstanceId\":{firebaseAppInstanceId},\"idfa\":{idfa},\"idfv\":{idfv},\"adid\":{adid},\"gpsAdid\":{gpsAdid},\"afid\":{afid}}}";
        }

    }
}