
namespace Guru
{
    using System;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Networking;
    
    public class GoogleOrderRequest : RequestBase
    {
        public string token;
        public GoogleOrderData orderData;

        public GoogleOrderRequest(){}
        
        protected override string RequestURL => ServerConst.API_ORDER_ANDROID;
        protected override UnityWebRequest CreateRequest()
        {
            this.Log($"send orderData:{orderData}"); 
            var request = new UnityWebRequest(RequestURL, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(orderData.ToJson()));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader(ServerConst.HEADER_PARAM_CONTENT_TYPE, ServerConst.HEADER_CONTENT_TYPE_VALUE);
            request.SetRequestHeader(ServerConst.HEADER_PARAM_APPID, IPMConfig.IPM_X_APP_ID);
            request.SetRequestHeader(ServerConst.HEADER_PARAM_UID, IPMConfig.IPM_UID);
            request.SetRequestHeader(ServerConst.HEADER_PARAM_ACCESS_TOKEN, IPMConfig.IPM_AUTH_TOKEN);
            return request;
        }

        protected override void RequestSuccessCallBack(string response)
        {
            try
            {
                Debug.Log($"[IAP] --- Google Order Response: {response}");
                ResponseData<OrderResponse> responseData = JsonUtility.FromJson<ResponseData<OrderResponse>>(response);
                if (responseData != null && responseData.data != null)
                {
                    double usdPrice = responseData.data.usdPrice;
                    bool isTest = responseData.data.test;
                    
                    Analytics.ReportIAPSuccessEvent(orderData, usdPrice, isTest);
                }
            }
            catch (Exception ex)
            {
                Analytics.LogCrashlytics(ex);
            }
        }
        
        
        public static GoogleOrderRequest Build(int orderType, string productId, 
            string token, string orderId, string date, int level, 
            string userCurrency, double payPrice, string scene, bool isFree = false,
            string offerId = "",  string basePlanId = "")
        {
            var request = new GoogleOrderRequest()
            {
                orderData = new GoogleOrderData(orderType, productId, token, orderId, date, level, 
                    userCurrency, payPrice, scene, isFree, offerId, basePlanId),
                token = token,
            };
            return request;
        }
        
        public static  GoogleOrderRequest Build(GoogleOrderData data)
        {
            var request = new GoogleOrderRequest()
            {
                orderData = data,
                token = data.token,
            };
            return request;
        }
        
        
    }
}