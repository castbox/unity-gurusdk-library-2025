
namespace Guru
{
	using System.Text;
	using Guru.LitJson;
	using UnityEngine;
	using UnityEngine.Networking;
	
	public class RefreshTokenRequest : RequestBase
	{
		protected override string RequestURL => ServerConst.API_AUTH_RENEW_TOKEN;
		protected override UnityWebRequest CreateRequest()
		{
			var request = new UnityWebRequest(RequestURL, "POST");
			//request.uploadHandler = new UploadHandlerRaw();
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader(ServerConst.HEADER_PARAM_CONTENT_TYPE, ServerConst.HEADER_CONTENT_TYPE_VALUE);
			request.SetRequestHeader(ServerConst.HEADER_PARAM_APPID, IPMConfig.IPM_X_APP_ID);
			request.SetRequestHeader(ServerConst.HEADER_PARAM_UID, IPMConfig.IPM_UID);
			request.SetRequestHeader(ServerConst.HEADER_PARAM_ACCESS_TOKEN, IPMConfig.IPM_AUTH_TOKEN);
			return request;
		}

		protected override void RequestSuccessCallBack(string response)
		{
			ResponseData<TokenResponse> responseData =
				JsonUtility.FromJson<ResponseData<TokenResponse>>(response);
			
			this.Log(response);
			this.Log(responseData.data.ToString());
			IPMConfig.IPM_AUTH_TOKEN = responseData.data.token;
			IPMConfig.IPM_AUTH_TOKEN_TIME = TimeUtil.GetCurrentTimeStampSecond();
			this.Log($"[SDK] --- RefreshTokenRequest Success: {responseData.data.token}");
		}
	}
}