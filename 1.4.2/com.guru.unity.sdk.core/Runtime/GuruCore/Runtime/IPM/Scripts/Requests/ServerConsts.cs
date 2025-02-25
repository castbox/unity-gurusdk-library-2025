namespace Guru
{
    /// <summary>
    /// 中台服务
    /// </summary>
    public static class ServerConst
    {
        // 中台服务 HOSTNAME		
#if DEBUG
        public const string API_HOST = "https://dev.saas.castbox.fm";
#else
		public const string API_HOST = "https://saas.castbox.fm";
#endif
        // --- Parameters ---
        public const string HEADER_PARAM_APPID = "X-APP-ID";
        public const string HEADER_PARAM_DEVICE_INFO = "X-DEVICE-INFO";
        public const string HEADER_PARAM_UID = "X-UID";
        public const string HEADER_PARAM_ACCESS_TOKEN = "X-ACCESS-TOKEN";
        public const string HEADER_PARAM_CONTENT_TYPE = "Content-Type";
        public const string HEADER_CONTENT_TYPE_VALUE = "application/json";
        
        public const string POST_PARAM_ACCESS_TOKEN = "accessToken";
        public const string POST_PARAM_ID_TOKEN = "idToken";
        public const string POST_PARAM_TOKEN = "token";
        public const string POST_PARAM_PROVIDER = "provider";
        public const string POST_PARAM_CLIENT_TYPE = "clientType";
        public const string POST_PARAM_TOKEN_SECRET = "tokenSecret";


        // --- Server API ---
        // docurl: https://github.com/castbox/backend-dev/blob/main/saas/%E4%B8%AD%E5%8F%B0%E6%9C%8D%E5%8A%A1%E6%8E%A5%E5%85%A5%E6%89%8B%E5%86%8C.md
        public static string API_AUTH_SECRET => GetFullUrl("auth/api/v1/tokens/provider/secret");
        public static string API_AUTH_RENEW_FIREBASE => GetFullUrl("auth/api/v1/renewals/firebase");
        public static string API_AUTH_RENEW_TOKEN => GetFullUrl("auth/api/v1/renewals/token");
        public static string API_ORDER_ANDROID => GetFullUrl("order/api/v1/orders/android");
        public static string API_ORDER_IOS => GetFullUrl("order/api/v1/orders/ios");
        public static string API_DEVICE_UPLOAD => GetFullUrl("device/api/v1/devices");
        public static string API_ORDER_SUBSCRIBE_VALIDITY_STATUS => GetFullUrl("order/api/v1/orders/subscribe/validity_status");
        public static string API_PUSH_APP_EVENT => GetFullUrl("push/api/v1/push/app/event");

        /// <summary>
        /// 获取完整的 API 链接
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetFullUrl(string url)
        {
            if (url.StartsWith("/")) url = url.Substring(1);
            return $"{API_HOST}/{url}";
        }
    }
}