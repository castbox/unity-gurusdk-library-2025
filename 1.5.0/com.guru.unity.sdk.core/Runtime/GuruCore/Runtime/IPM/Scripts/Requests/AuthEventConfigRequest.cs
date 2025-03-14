using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Guru
{
    /// <summary>
    /// 2025-3-13
    /// 【产研】优化用户属性补全逻辑
    /// https://www.tapd.cn/33527076/prong/stories/view/1133527076001025453?from_iteration_id=1133527076001003267
    /// </summary>
    public class AuthEventConfigRequest : RequestBase
    {
        protected override string RequestURL => ServerConst.API_AUTH_EVENT_CONFIG;
        
        protected override UnityWebRequest CreateRequest()
        {
            EventConfig eventConfig = new EventConfig();
            UnityEngine.Debug.Log($"[SDK] --- Send EventConfig:{eventConfig}");
            var request = new UnityWebRequest(RequestURL, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(eventConfig)));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader(ServerConst.HEADER_PARAM_CONTENT_TYPE, ServerConst.HEADER_CONTENT_TYPE_VALUE);
            request.SetRequestHeader(ServerConst.HEADER_PARAM_APPID, IPMConfig.IPM_X_APP_ID);
            request.SetRequestHeader(ServerConst.HEADER_PARAM_ACCESS_TOKEN, IPMConfig.IPM_AUTH_TOKEN);
            return request;
        }
        
        
        protected override void RequestSuccessCallBack(string response)
        {
            UnityEngine.Debug.Log("[SDK] --- Send EventConfig Success");
        }
    }
}
