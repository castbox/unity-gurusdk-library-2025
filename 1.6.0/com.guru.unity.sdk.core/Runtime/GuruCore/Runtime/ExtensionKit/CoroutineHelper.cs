namespace Guru
{
	using System;
	using System.Collections;
	using UnityEngine;
	
	[MonoSingleton(EMonoSingletonType.CreateOnNewGameObject, false)]
	public sealed class CoroutineHelper : MonoSingleton<CoroutineHelper>
	{
		public Coroutine Begin(IEnumerator enumerator)
		{
			return StartCoroutine(enumerator);
		}

		public void Stop(Coroutine coroutine)
		{
			StopCoroutine(coroutine);
		}

		public Coroutine StartDelayed(WaitForSeconds delay, Action callback)
		{
			return ((MonoBehaviour)this).StartDelayed(delay, callback);
		}
		
		public Coroutine StartDelayed(WaitForSecondsRealtime delay, Action callback)
		{
			return ((MonoBehaviour)this).StartDelayed(delay, callback);
		}

		public Coroutine StartDelayed(float delay, Action callback)
		{
			return ((MonoBehaviour)this).StartDelayed(delay, callback);
		}

		public Coroutine StartDelayedWithFrame(int framesOfDelay, Action callback)
		{
			return ((MonoBehaviour)this).StartDelayed(framesOfDelay, callback);
		}
	}

}