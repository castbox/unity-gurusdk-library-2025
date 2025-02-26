

using System;

namespace Guru
{
    using System.Text;
    using UnityEngine;
    using UnityEngine.Networking;
    
    public class AppleOrderRequest : RequestBase
    {
        public string transactionID;
        public AppleOrderData orderData;

        public AppleOrderRequest(){}
        
        
        protected override string RequestURL => ServerConst.API_ORDER_IOS;
        protected override UnityWebRequest CreateRequest()
        {
            this.Log($"send orderData:{orderData}");
            var request = new UnityWebRequest(RequestURL, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(orderData.ToJson()));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader(ServerConst.HEADER_PARAM_APPID, IPMConfig.IPM_X_APP_ID);
            request.SetRequestHeader(ServerConst.HEADER_PARAM_UID, IPMConfig.IPM_UID);
            request.SetRequestHeader(ServerConst.HEADER_PARAM_ACCESS_TOKEN, IPMConfig.IPM_AUTH_TOKEN);
            request.SetRequestHeader(ServerConst.HEADER_PARAM_CONTENT_TYPE, ServerConst.HEADER_CONTENT_TYPE_VALUE);
            return request;
        }

        protected override void RequestSuccessCallBack(string response)
        {
            try
            {
                Debug.Log($"[IAP] --- Apple Order Response: {response}");
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
        
        public static AppleOrderRequest Build(int orderType, string productId, 
            string receipt, string orderId, string date, int level,
            string userCurrency, double payPrice, string scene, string bundleId, string idfv, bool isFree = false, string offerId = "", string basePlanId = "")
        {
            var request = new AppleOrderRequest()
            {
                transactionID = orderId,
                orderData = new AppleOrderData(orderType, productId, receipt, orderId, date, level,
                    userCurrency, payPrice, scene, bundleId, idfv, isFree, offerId, basePlanId),
            };
            return request;
        }

        public static AppleOrderRequest Build(AppleOrderData data)
        {
            var request = new AppleOrderRequest()
            {
                orderData = data,
                transactionID = data.orderId,
            };
            return request;
        }
    }
}