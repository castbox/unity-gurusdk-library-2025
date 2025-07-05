using Cysharp.Threading.Tasks;

namespace Guru
{
    /// <summary>
    /// 设置 Consent 数据
    /// </summary>
    public partial class Analytics
    {
        /// <summary>
        /// 三方自定义预初始化
        /// 参数设置后，各归因平台尚不能启动平台SDK
        /// 中间需要等待 Consent 数据输入
        /// </summary>
        public static void PrepareCustomDrivers() => _customDriverManager.Prepare();
        
        /// <summary>
        /// 设置和更新 Consent 数据
        /// </summary>
        /// <param name="consentData"></param>
        public static void DispatchConsentData(ConsentData consentData)
        {
            _customDriverManager.SetConsentData(consentData);
        }
        
        /// <summary>
        /// 三方自定义初始化
        /// </summary>
        public static UniTask InitCustomDrives() => _customDriverManager.Initialize();

        /// <summary>
        /// 追加自定义事件管理
        /// </summary>
        /// <param name="driver"></param>
        public static void AddCustomDriver(CustomEventDriver driver)
        {
            _customDriverManager.AddCustomDriver(driver);
        }


    }
}