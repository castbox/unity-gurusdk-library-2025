using System;
using System.Collections;
using UnityEngine;

public static class UnityExtensions
{
	#region Monobehaviour Extension
	public static Coroutine StartDelayed(this MonoBehaviour monoBehaviour, WaitForSeconds delay, Action callback)
	{
		return monoBehaviour.StartCoroutine(DelayedFire(delay, callback));
	}
	
	public static Coroutine StartDelayed(this MonoBehaviour monoBehaviour, float delay, Action callback)
	{
		return monoBehaviour.StartCoroutine(DelayedFire(delay, callback));
	}
    
	public static Coroutine StartDelayed(this MonoBehaviour monoBehaviour, int frames, Action callback)
	{
		return monoBehaviour.StartCoroutine(DelayedFire(frames, callback));
	}
	
	public static Coroutine StartDelayed(this MonoBehaviour monoBehaviour, WaitForSecondsRealtime realTime, Action callback)
	{
		return monoBehaviour.StartCoroutine(DelayedFire(realTime, callback));
	}
    
	private static IEnumerator DelayedFire(WaitForSeconds delay, Action callback)
	{
		yield return delay;
		callback?.Invoke();
	}
	
	private static IEnumerator DelayedFire(WaitForSecondsRealtime delay, Action callback)
	{
		yield return delay;
		callback?.Invoke();
	}
	
	private static IEnumerator DelayedFire(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		callback?.Invoke();
	}
    
	private static IEnumerator DelayedFire(int frames, Action callback)
	{
		while (frames > 0)
		{
			yield return null;
			frames--;
		}
		callback?.Invoke();
	}
	#endregion

	#region string Extension

	public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

	#endregion
}
