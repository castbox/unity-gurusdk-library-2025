namespace Guru
{
    public partial class GuruSDK
    {
        /// <summary>
        /// 显示In-App-Review
        /// </summary>
        public static void ShowRating() => GuruRating.ShowRating();

        /// <summary>
        /// 设置邮件格式
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public static void SetEmailFeedback(string email, string subject, string body = "")
            => GuruRating.SetEmailFeedback(email, subject, body);


    }
}