

namespace Guru
{
	using System;
	using System.Collections;
	using UnityEngine;
	using UnityEngine.Networking;
	
	public abstract class RequestBase
	{
		protected abstract string RequestURL { get; }
		protected abstract UnityWebRequest CreateRequest();
		protected abstract void RequestSuccessCallBack(string response);
		
		private WaitForSeconds _waitTime = new WaitForSeconds(5);

		private int _retryTimes = 3;
		private int _timeOut = 90;
		private int _currentRetryTimes;
		private Action _onSuccessCallBack;
		private Action _onFailCallBack;
		private readonly string TAG = "IPM";

		public RequestBase SetRetryTimes(int retryTimes)
		{
			_retryTimes = retryTimes;
			return this;
		}
		
		public RequestBase SetRetryWaitSeconds(int waitSeconds)
		{
			_waitTime = new WaitForSeconds(waitSeconds);;
			return this;
		}
		
		public RequestBase SetTimeOut(int timeOut)
		{
			_timeOut = timeOut;
			return this;
		}

		public RequestBase SetSuccessCallBack(Action successCallBack)
		{
			_onSuccessCallBack = successCallBack;
			return this;
		}
		
		public RequestBase SetFailCallBack(Action failCallBack)
		{
			_onFailCallBack = failCallBack;
			return this;
		}
		
		public virtual void Send()
		{
			var request = CreateRequest();
			if (request == null)
				return;

			request.timeout = _timeOut;
			CoroutineHelper.Instance.StartCoroutine(SendRequest(request));
		}
		
		private IEnumerator SendRequest(UnityWebRequest request)
		{
			yield return request.SendWebRequest();
			
			if (request.result != UnityWebRequest.Result.Success)
			{
				_currentRetryTimes++;
				this.LogError($"{request.url} reqeust fail. /n [responseCode:{request.responseCode}, result:{request.result}, error:{request.error}]");
				if (_retryTimes > 0 && _currentRetryTimes >= _retryTimes)
				{
					this.LogError( $"{TAG} {request.url} 请求超出重试次数限制，请求失败");
					_onFailCallBack?.Invoke();
					ClearRetry();
				}
				else
				{
					yield return _waitTime;
					Send();
				}
			}
			else
			{
				string response = request.downloadHandler.text;
				this.Log($"{TAG} {RequestURL} response : {response}");
				RequestSuccessCallBack(response);
				_onSuccessCallBack?.Invoke();
				ClearRetry();
			}
		}

		private void ClearRetry()
		{
			_currentRetryTimes = 0;
		}
	}
}