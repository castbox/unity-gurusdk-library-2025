
#if GURU_ADJUST
namespace Guru
{
	using System.Collections.Generic;
	using AdjustSdk;
	
	//Adjust上报事件封装
	public partial class Analytics
	{
		private static Dictionary<string, string> InitAdjustEventTokenDict()
		{
			var dict = new Dictionary<string, string>();

			foreach (var adjustEvent in GuruSettings.Instance.AnalyticsSetting.AdjustEventList)
			{
#if UNITY_ANDROID || UNITY_EDITOR
				dict[adjustEvent.EventName] = adjustEvent.AndroidToken;
#elif UNITY_IOS
				dict[adjustEvent.EventName] = adjustEvent.IOSToken;
#endif
			}

			return dict;
		}

		internal static AdjustEvent CreateAdjustEvent(string eventName)
		{
			string tokenID = GetAdjustEventToken(eventName);
			if (string.IsNullOrEmpty(tokenID))
			{
				return null;
			}
			UnityEngine.Debug.Log($"{TAG} --- Send Adjust Event: {eventName}({tokenID})");
			return new AdjustEvent(tokenID);
		}


		private static Dictionary<string, string> _adjustEventTokenDict;
		public static string GetAdjustEventToken(string eventName)
		{
			if (_adjustEventTokenDict == null)
			{
				_adjustEventTokenDict = InitAdjustEventTokenDict();
			}
			
			if (_adjustEventTokenDict.TryGetValue(eventName, out var token))
			{
				return token;
			}
			else
			{
				UnityEngine.Debug.LogWarning($"{TAG} --- AdjustEventTokenDict token not found for:{eventName}");
				return null;
			}
		}
	}
	
	internal static class AdjustEventExtension
	{
		public static AdjustEvent AddEventParameter(this AdjustEvent adjustEvent, string key, string value)
		{
			adjustEvent.AddCallbackParameter(key, value);
			adjustEvent.AddPartnerParameter(key, value);
			return adjustEvent;
		}
	}
	
}
#endif