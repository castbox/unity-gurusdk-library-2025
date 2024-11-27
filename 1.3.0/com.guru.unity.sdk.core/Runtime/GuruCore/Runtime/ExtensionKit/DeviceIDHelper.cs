
namespace Guru
{
    public class DeviceIDHelper
    {
        /// <summary>
        /// UUID (V4) 
        /// </summary>
        /// <returns></returns>
        public static string UUID => System.Guid.NewGuid().ToString("D");
        

        /// <summary>
        /// IOS 或者 IDFV
        /// 当获取到非法的值时, 用 UUID 代替
        /// </summary>
        /// <returns></returns>
        public static string IDFV
        {
            get
            {
                var idfv = UnityEngine.SystemInfo.deviceUniqueIdentifier;
                if (string.IsNullOrEmpty(idfv) || idfv.Contains("0000-0000-0000"))
                {
                    idfv = UUID;
                }
                return idfv;
            }
            
        }

        /// <summary>
        /// Android ID
        /// 可通过Unity 本身的接口来获取
        /// </summary>
        /// <returns></returns>
        public static string AndroidID
        {
            get
            {
                var aid = UnityEngine.SystemInfo.deviceUniqueIdentifier;
                if (string.IsNullOrEmpty(aid)) aid = UUID;
                return aid;
            }
        }



    }
}