namespace Guru
{
    using System;
#if UNITY_ANDROID
    using Google.Play.Review;
#endif

#if UNITY_IOS
    using UnityEngine.iOS;
#endif
    using UnityEngine;
    using UnityEngine.Networking;
    
    /// <summary>
    /// 评价管理器
    /// </summary>
    public class GuruRating: Singleton<GuruRating>
    {

        public static bool ShowLog { get; set; } = false;

        #region 初始化

        protected override void Init()
        {
            base.Init();
        }

        #endregion
        
        #region 评价显示
        
#if UNITY_ANDROID

        private bool _isOnRating = false;
        /// <summary>
        /// 显示Google的评价
        /// </summary>
        private void ShowGoogleRating()
        {
            if (_isOnRating) return;
            _isOnRating = true;
            OnGoogleRating();
        }
        
        /// <summary>
        /// 启动Google评价流程
        /// </summary>
        private void OnGoogleRating()
        {
            ReviewManager rm = new ReviewManager();
            rm.RequestReviewFlow().Completed += pao =>
            {
                // 请求 PlayReviewInfo 对象
                if (pao.Error != ReviewErrorCode.NoError)
                {
                    if(ShowLog) Debug.Log($"ReviewInfo Error: {pao.Error.ToString()}");
                    OnGoogleRatingResult(false);
                }
                else
                {
                    // 启动应用内评价流程, 调用后可以不用关心后继的结果. 游戏继续流程即可
                    rm.LaunchReviewFlow(pao.GetResult()).Completed += lao =>
                    {
                        bool result = true;
                        if (lao.Error != ReviewErrorCode.NoError)
                        {
                            if(ShowLog) Debug.Log($"LaunchReview Error: {lao.Error.ToString()}");
                            result = false;
                        }
                        OnGoogleRatingResult(result);
                        rm = null;
                    };
                }
            };
        }

        private void OnGoogleRatingResult(bool success)
        {
            // On getting review result
            if(ShowLog) Debug.Log($"Google Review flow ends, result: {success}");
            _isOnRating = false;
        }

#endif
        
#if UNITY_IOS
        /// <summary>
        /// 显示苹果的Rating
        /// </summary>
        private void ShowAppleRating() => Device.RequestStoreReview();  // 显示苹果的评价面板
#endif
        
        #endregion

        #region 发送邮件

        /// <summary>
        /// 设置邮件显示
        /// </summary>
        /// <param name="email"></param>
        /// <param name="body"></param>
        private void SendEmail(string email, string subject = "", string body = null)
        {
            if (string.IsNullOrEmpty(subject)) subject = GetDefaultMailTitle();
            if (string.IsNullOrEmpty(body)) body = GetDefaultMailBody();
            
            string url = $"mailto:{email}?subject={EscapeUrl(subject)}&body={EscapeUrl(body)}";
            Debug.Log($"Send Emailto: {url}");
            Application.OpenURL(url);
        }
        
        /// <summary>
        /// 获取默认的邮件标题
        /// </summary>
        /// <returns></returns>
        private string GetDefaultMailTitle()
        {
            return $"Some suggestions for improving {GuruSettings.Instance.ProductName}";
        }
        
        /// <summary>
        /// 获取默认的邮件内容模版
        /// </summary>
        private string GetDefaultMailBody()
        {
            string body = "Please Enter your message here\n\n\n\n" +
                          "________" +
                          "\n\nPlease Do Not Modify This\n\n" +
                          "Game: " + GuruSettings.Instance.ProductName + "\n\n" +
                          "Model: " + SystemInfo.deviceModel + "\n\n" +
                          "OS: " + SystemInfo.operatingSystem + "\n\n" +
                          "UserId: " + IPMConfig.IPM_UID + "\n\n" +
                          "Version: " + Application.version + "\n\n" +
                          "________";
            return body;
        }



        #endregion
        
        #region 工具接口

        /// <summary>
        /// 转换为URL编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EscapeUrl(string str)
        {
            return UnityWebRequest.EscapeURL(str).Replace("+", "%20");
        }
        
        

        #endregion

        #region 公开接口

        /// <summary>
        /// 显示评价面板
        /// </summary>
        public static void ShowRating()
        {
#if UNITY_EDITOR
            Console.WriteLine($"Editor is Calling ShowRating api...");
#elif UNITY_ANDROID
            Instance.ShowGoogleRating();
#elif UNITY_IOS
            Instance.ShowAppleRating();
#endif
        }


        /// <summary>
        /// 设置邮件反馈
        /// </summary>
        /// <param name="email"></param>
        /// <param name="body"></param>
        public static void SetEmailFeedback(string email, string subject = "", string body = "")
        {
            Instance.SendEmail(email, subject, body);
        }
        
        #endregion
        
    }
}