namespace Guru
{
	using System.Text;
	using Firebase.Crashlytics;
	using UnityEngine;
	using UnityEngine.Networking;
	using System.Collections.Generic;
	using Newtonsoft.Json;
	
	/// <summary>
	/// 中台匿名授权用户请求
	/// https://docs.google.com/document/d/1yRE0HQTwaDfBeH7Zd1li1Xr8JIdfqkz3ixqdjD-aH44/edit#heading=h.eispbvmfw5oo
	/// </summary>
	public class AuthUserRequest : RequestBase
	{
		protected override string RequestURL => ServerConst.API_AUTH_SECRET;

		protected override UnityWebRequest CreateRequest()
		{
			var data = new Dictionary<string, object>()
			{
				["secret"] = IPMConfig.IPM_DEVICE_ID,
				["eventConfig"] = EventConfig.Build(),
			};
			var  json = JsonConvert.SerializeObject(data);
			Debug.Log($"[SDK] --- AuthUserRequest json: {json}");
			var request = new UnityWebRequest(RequestURL, "POST");
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader(ServerConst.HEADER_PARAM_CONTENT_TYPE, ServerConst.HEADER_CONTENT_TYPE_VALUE);
			request.SetRequestHeader(ServerConst.HEADER_PARAM_APPID, IPMConfig.IPM_X_APP_ID);
			request.SetRequestHeader(ServerConst.HEADER_PARAM_DEVICE_INFO, new DeviceInfoData().ToString());
			return request;
		}

		protected override void RequestSuccessCallBack(string response)
		{
			ResponseData<TokenResponse> responseData =
				JsonUtility.FromJson<ResponseData<TokenResponse>>(response);

			if (responseData == null || responseData.data == null)
				return;
			
			this.Log(response);
			this.Log(responseData.data.ToString());
			
			Analytics.SetFirebaseUserId(responseData.data.uid);
			
			IPMConfig.IPM_UID = responseData.data.uid;
			IPMConfig.IPM_UID_INT = responseData.data.uidInt;
			IPMConfig.IPM_AUTH_TOKEN = responseData.data.token;
			IPMConfig.FIREBASE_AUTH_TOKEN = responseData.data.firebaseToken;
			IPMConfig.USER_CREATED_TIMESTAMP = $"{responseData.data.createdAtTimestamp}";
			IPMConfig.IPM_NEW_USER = responseData.data.newUser;
			IPMConfig.IPM_AUTH_TOKEN_TIME = TimeUtil.GetCurrentTimeStampSecond();
			IPMConfig.IPM_FIREBASE_TOKEN_TIME = TimeUtil.GetCurrentTimeStampSecond();
			IPMConfig.IPM_UUID = IDHelper.GenUUID(responseData.data.uid);
			DeviceUtil.Save2AppGroup();
		}
		
	}
}