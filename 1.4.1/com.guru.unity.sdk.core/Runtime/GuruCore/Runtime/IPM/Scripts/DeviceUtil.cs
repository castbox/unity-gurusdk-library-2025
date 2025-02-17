namespace Guru
{
	using System;
	using UnityEngine;
	
#if UNITY_IOS
	using System.Runtime.InteropServices;
	using UnityEngine.iOS;
#endif
	
	
    public static class DeviceUtil
    {
        public static bool IsGetDeviceInfoSuccess;
#if UNITY_IOS
        [DllImport ("__Internal")]
        private static extern string iOSDeviceInfo();
        [DllImport ("__Internal")]
        private static extern void iOSSetBadge();
        [DllImport ("__Internal")]
        private static extern void savePlayerPrefs2AppGroup(string appGroupName);
        [DllImport ("__Internal")]
        private static extern void iOSClearBadge();
#endif
	    public static bool GetDeviceInfo()
        {
	        if (!IsGetDeviceInfoSuccess)
	        {
#if UNITY_EDITOR
		        GetEditorDeviceInfo();
#elif UNITY_ANDROID
		        GetAndroidDeviceInfo();
#elif UNITY_IOS
		        GetIOSDeviceInfo();
#endif
	        }
	        return IsGetDeviceInfoSuccess;
        }

        #region IOS
        
        private static void GetIOSDeviceInfo()
        {
#if UNITY_IOS 
			try
			{
				Debug.Log($"[SDK] --- GetIOSDeviceInfo:: iOSDeviceInfo<string>");
				string content = iOSDeviceInfo();
				Debug.Log($"GetDeviceInfo:{content}");
				if(!string.IsNullOrEmpty(content))
				{
					string[] infos = content.Split('$');
					// IPMConfig.SetDeviceId(infos[0]);
					IPMConfig.IPM_APP_VERSION = infos[1];
					IPMConfig.IPM_TIMEZONE = infos[2];
					IPMConfig.IPM_MODEL = infos[3];
					IPMConfig.IPM_LANGUAGE = infos[4];
					IPMConfig.IPM_LOCALE = infos[5];
					IPMConfig.IPM_COUNTRY_CODE = infos[6];
					IsGetDeviceInfoSuccess = true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}
#endif
        }
		
        public static void SetiOSBadge()
        {
#if UNITY_IOS && !UNITY_EDITOR
            iOSSetBadge();
#endif
        }

        public static void Save2AppGroup()
        {
#if UNITY_IOS && !UNITY_EDITOR
            savePlayerPrefs2AppGroup(IPMConfig.IPM_IOS_APP_GROUP);
#endif
        }
		
        public static void ClerBadge()
        {
#if UNITY_IOS && !UNITY_EDITOR
            iOSClearBadge();
#endif
        }

        #endregion
        
        #region Android
        
        private static AndroidJavaObject _androidJavaObject;

        public static AndroidJavaObject U3D2Android
        {
	        get
	        {
		        if (_androidJavaObject == null)
		        {
			        _androidJavaObject = new AndroidJavaObject("com.guru.u3d2android.u3d2android");
		        }
		        return _androidJavaObject;
	        }
        }


        private static void GetAndroidDeviceInfo()
        {
#if UNITY_ANDROID
	        try
	        {
		        if (U3D2Android != null)
		        {
			        Debug.Log($"[SDK] --- GetAndroidDeviceInfo:: com.guru.u3d2android.u3d2android: getDeviceInfo<string>");
			        string content = U3D2Android.Call<string>("getDeviceInfo");
			        Debug.Log($"GetDeviceInfo:{content}");
			        if(!string.IsNullOrEmpty(content))
			        {
				        string[] infos = content.Split('$');
				        IPMConfig.IPM_BRAND = infos[0];
				        IPMConfig.IPM_LANGUAGE = infos[1];
				        IPMConfig.IPM_MODEL = infos[2];
				        IPMConfig.IPM_TIMEZONE = infos[4];
				        IPMConfig.IPM_LOCALE = infos[5];
				        IPMConfig.IPM_COUNTRY_CODE = infos[6];
				        IsGetDeviceInfoSuccess = true;
			        }
		        }
	        }
	        catch (Exception ex)
	        {
		        Debug.LogError(ex);
	        }
#endif
        }
        
        
        #endregion

        #region Editor

        private static void GetEditorDeviceInfo()
        {
	        var cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
	        
	        IPMConfig.IPM_BRAND = "PC";
	        IPMConfig.IPM_LANGUAGE = cultureInfo.ThreeLetterISOLanguageName;
	        IPMConfig.IPM_MODEL = SystemInfo.deviceModel;
	        IPMConfig.IPM_TIMEZONE = $"{TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow)}";
	        IPMConfig.IPM_LOCALE = cultureInfo.ThreeLetterWindowsLanguageName;
	        IPMConfig.IPM_COUNTRY_CODE = cultureInfo.TwoLetterISOLanguageName;
	        IsGetDeviceInfoSuccess = true;
        }


        #endregion
        
        
        #region 系统弹框
        public static void ShowToast(string content)
        {
#if UNITY_EDITOR
	        UnityEditor.EditorUtility.DisplayDialog("系统提示", content, "OK");
#elif UNITY_ANDROID
			if (_androidJavaObject == null) return;
			_androidJavaObject.Call<bool>("showToast", content);
#endif
	        Debug.Log($"--------- INFORMATION --------\n{content}\n--------- INFORMATION --------");
        }
        

        #endregion

        #region 系统版本
        
        /// <summary>
        /// 获取AndroidOS系统的版本号
        /// 如果获取失败则返回 0
        /// </summary>
        /// <returns></returns>
        public static int GetAndroidOSVersionInt()
        {
#if UNITY_EDITOR
			return 0;
#elif UNITY_ANDROID
			return U3D2Android?.CallStatic<int>("getSystemVersionSdkInt") ?? 0;
#elif UNITY_IOS
			return 0;
#endif
        }
        

        public static string GetIOSOSVersionString()
        {
#if UNITY_EDITOR
			return "0";
#elif UNITY_ANDROID
			return "0";
#elif UNITY_IOS
			return Device.systemVersion;
#endif
        }

        #endregion
    }
}