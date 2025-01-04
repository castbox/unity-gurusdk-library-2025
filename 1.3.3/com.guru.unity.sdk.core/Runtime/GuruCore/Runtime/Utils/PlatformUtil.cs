using System.IO;
using UnityEngine;

public static class PlatformUtil
{
	public static string GetPlatformName()
	{
#if UNITY_IOS
		return "iOS";
#else
		return "Android";
#endif
	}
	
	public static bool IsEnableLog()
	{
#if ENABLE_LOG
		return true;
#else
		return false;
#endif
	}
	
	public static bool IsDebug()
	{
#if DEBUG || UNITY_EDITOR
		return true;
#else
		return false;
#endif
	}
}
