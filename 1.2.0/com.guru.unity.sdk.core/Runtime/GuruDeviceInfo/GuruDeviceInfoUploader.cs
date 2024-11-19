using System;

namespace Guru
{
    /// <summary>
    /// Guru 消息推送管理器
    /// </summary>
    public class GuruDeviceInfoUploader
    {
        private const string LOG_TAG = "[SDK][PUSH]";
        
        private static GuruDeviceInfoUploader _instance;
        public static GuruDeviceInfoUploader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GuruDeviceInfoUploader();
                }
                return _instance;
            }
        }

        private int _retryTimes = 0;
        private bool _pushServiceEnabled;
        private Action _onUploadSuccessHandler;

        private GuruDeviceInfoUploader()
        {
            _pushServiceEnabled = true; // 推送需求默认开启
        }

        public void SetPushNotificationEnabled(bool enabled, Action onUploadSuccess = null)
        {
            _pushServiceEnabled = enabled;
            Upload(onUploadSuccess);
        }

        /// <summary>
        /// 上报设备信息
        /// </summary>
        public void Upload(Action onUploadSuccess = null)
        {
            _onUploadSuccessHandler = onUploadSuccess;
            
            if (!NetworkUtil.IsNetAvailable)
            {
                RetryUploadRequest();
            }
            else
            {
                SendUploadRequest();
            }
        }

        private void SendUploadRequest()
        {
            Log.I(LOG_TAG, "Send UploadDeviceInfo");
            // 直接上传
            new DeviceInfoUploadRequest(_pushServiceEnabled)
                .SetRetryTimes(1)
                .SetSuccessCallBack(OnUploadSuccess)
                .SetFailCallBack(OnUploadFail)
                .Send();
        }


        private void OnUploadSuccess()
        {
            _retryTimes = 0;
            Log.I(LOG_TAG, "UploadDeviceInfo Success!!");
            _onUploadSuccessHandler?.Invoke();
        }
        
        private void OnUploadFail()
        {
            RetryUploadRequest();
        }

        /// <summary>
        /// 上传中
        /// </summary>
        private void RetryUploadRequest()
        {
            float retryDelay = (float)Math.Pow(2, _retryTimes);
            _retryTimes++;
            CoroutineHelper.Instance.StartDelayed(retryDelay, SendUploadRequest);
        }

    }
}