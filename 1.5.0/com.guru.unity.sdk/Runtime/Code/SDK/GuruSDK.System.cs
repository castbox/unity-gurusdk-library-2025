using System;
using UnityEngine;

namespace Guru
{
    public partial class GuruSDK
    {
        #region System
        
        /// <summary>
        /// 打开页面
        /// </summary>
        /// <param name="url"></param>
        public static void OpenURL(string url)
        {
            GuruWebview.OpenPage(url);
        }

        /// <summary>
        /// 打开隐私协议页面
        /// </summary>
        public static void OpenPrivacyPage()
        {
            if (string.IsNullOrEmpty(PrivacyUrl))
            {
                LogE("PrivacyUrl is null"); 
                return;
            }

            OpenURL(PrivacyUrl);
        }
        
        /// <summary>
        /// 打开服务条款页面
        /// </summary>
        public static void OpenTermsPage()
        {
            if (string.IsNullOrEmpty(TermsUrl))
            {
                LogE("TermsUrl is null"); 
                return;
            }
            OpenURL(TermsUrl);
        }
        
        #endregion
        
        #region Android System

#if UNITY_ANDROID
        
        /// <summary>
        /// 获取 AndroidSDK 的系统版本号
        /// </summary>
        /// <returns></returns>
        public static int GetAndroidSystemVersion()
        {
            try
            {
                // sdkInt 是 Android SDK 的整数版本号,例如 Android 10 对应 29。
                // release 是 Android 版本的字符串表示,例如 "10"。
                using (AndroidJavaClass jc = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    int sdkInt = jc.GetStatic<int>("SDK_INT");
                    LogW($"[SDK] --- Android SDK Version:{sdkInt}");
                    return sdkInt;
                }
            }
            catch (Exception ex)
            {
                LogE(ex);
            }

            return 0;
        }

#endif

        #endregion

        #region Clear Data Cache

        /// <summary>
        /// 清除数据缓存
        /// </summary>
        public static void ClearData()
        {
            Model.ClearData();
            GuruIAP.Instance.ClearData();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
        
        #endregion
        
    }
}