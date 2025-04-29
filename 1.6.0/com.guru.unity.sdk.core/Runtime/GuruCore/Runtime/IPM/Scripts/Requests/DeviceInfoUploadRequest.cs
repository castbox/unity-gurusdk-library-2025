using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Guru
{
	
	/// <summary>
	/// 接口文档参见:
	/// <a>https://github.com/castbox/backend-dev/blob/main/saas/%E4%B8%AD%E5%8F%B0%E6%9C%8D%E5%8A%A1%E6%8E%A5%E5%85%A5%E6%89%8B%E5%86%8C.md#531%E8%AE%BE%E5%A4%87%E6%95%B0%E6%8D%AE%E4%B8%8A%E6%8A%A5</a>
	/// </summary>
	public class DeviceInfoUploadRequest : RequestBase
	{
		private bool _pushServiceEnabled;
		protected override string RequestURL => ServerConst.API_DEVICE_UPLOAD;
		
		public DeviceInfoUploadRequest(bool pushServiceEnabled = true)
		{
			_pushServiceEnabled = pushServiceEnabled;
		}
		
		protected override UnityWebRequest CreateRequest()
		{
			DeviceData deviceData = new DeviceData(_pushServiceEnabled);
			UnityEngine.Debug.Log($"[SDK] --- Send DeviceData:{deviceData}");
			var request = new UnityWebRequest(RequestURL, "POST");
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(deviceData)));
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader(ServerConst.HEADER_PARAM_CONTENT_TYPE, ServerConst.HEADER_CONTENT_TYPE_VALUE);
			request.SetRequestHeader(ServerConst.HEADER_PARAM_APPID, IPMConfig.IPM_X_APP_ID);
			request.SetRequestHeader(ServerConst.HEADER_PARAM_ACCESS_TOKEN, IPMConfig.IPM_AUTH_TOKEN);
			return request;
		}

		protected override void RequestSuccessCallBack(string response)
		{
			UnityEngine.Debug.Log("[SDK] --- Send DeviceData Success");
			IPMConfig.IS_UPLOAD_DEVICE_SUCCESS = true;
		}
		
		/// <summary>
		/// 设置是否打开推送
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public DeviceInfoUploadRequest SetPushEnabled(bool value = true)
		{
			_pushServiceEnabled = value;
			return this;
		}


	}
}