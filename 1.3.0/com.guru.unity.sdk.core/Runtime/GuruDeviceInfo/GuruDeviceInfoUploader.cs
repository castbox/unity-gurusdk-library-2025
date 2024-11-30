namespace Guru
{
    using System;
    
    /// <summary>
    /// Guru 消息推送管理器
    /// </summary>
    public class GuruDeviceInfoUploader
    {
        private const string LOG_TAG = "[SDK][DEVICE_INFO]";
        private const int MAX_RETRY_TIMES = 5; // 最大重试次数
        private const float BASE_RETRY_DELAY = 2f; // 基础重试延迟时间(秒)
        
        // 单例实现
        private static readonly Lazy<GuruDeviceInfoUploader> _lazyInstance = 
            new Lazy<GuruDeviceInfoUploader>(() => new GuruDeviceInfoUploader());
            
        public static GuruDeviceInfoUploader Instance => _lazyInstance.Value;

        // 状态标志位
        private int _retryTimes;
        private bool _pushServiceEnabled;
        private bool _isOnRequesting;
        private bool _hasUploadSuccessfully;
        
        
        // 回调事件
        private event Action OnUploadSuccessEvent;

        // 只读属性
        public bool HasUploadSuccessfully => _hasUploadSuccessfully;
        public bool IsOnRequesting => _isOnRequesting;
        
        private GuruDeviceInfoUploader()
        {
            InitializeState();
        }
        
        /// <summary>
        /// 初始化状态
        /// </summary>
        private void InitializeState()
        {
            _pushServiceEnabled = true; // 推送服务默认开启
            _isOnRequesting = false;
            _hasUploadSuccessfully = false;
            _retryTimes = 0;
        }

        /// <summary>
        /// 设置推送通知开关状态并触发上传
        /// </summary>
        /// <param name="enabled">是否启用推送</param>
        /// <param name="onUploadSuccess">上传成功回调</param>
        public void SetPushNotificationEnabled(bool enabled, Action onUploadSuccess = null)
        {
            _pushServiceEnabled = enabled;
            Upload(onUploadSuccess);
        }

        /// <summary>
        /// 上报设备信息
        /// </summary>
        /// <param name="onUploadSuccess">上传成功回调</param>
        public void Upload(Action onUploadSuccess = null)
        {
            if (onUploadSuccess != null)
            {
                OnUploadSuccessEvent += onUploadSuccess;
            }
            
            if (_isOnRequesting || _hasUploadSuccessfully) 
            {
                return;
            }

            if (!IsReadyToUpload())
            {
                ScheduleRetry();
                return;
            }

            SendUploadRequest();
        }
        
        /// <summary>
        /// 检查是否满足上传条件
        /// </summary>
        private bool IsReadyToUpload()
        {
            return NetworkUtil.IsNetAvailable 
                   && !string.IsNullOrEmpty(IPMConfig.IPM_UID) 
                   && !string.IsNullOrEmpty(IPMConfig.FIREBASE_PUSH_TOKEN);
        }

        /// <summary>
        /// 发送上传请求
        /// </summary>
        private void SendUploadRequest()
        {
            if (_isOnRequesting)
            {
                return;
            }

            _isOnRequesting = true;
            Log.I(LOG_TAG, "On sending device info...");
            
            new DeviceInfoUploadRequest(_pushServiceEnabled)
                .SetRetryTimes(1)
                .SetSuccessCallBack(HandleUploadSuccess)
                .SetFailCallBack(HandleUploadFailure)
                .Send();
        }


        /// <summary>
        /// 处理上传成功回调
        /// </summary>
        private void HandleUploadSuccess()
        {
            _hasUploadSuccessfully = true;
            _isOnRequesting = false;
            _retryTimes = 0;
            
            Log.I(LOG_TAG, "Upload success!");
            
            OnUploadSuccessEvent?.Invoke();
            CleanupCallbacks();
        }
        
        /// <summary>
        /// 处理上传失败回调
        /// </summary>
        private void HandleUploadFailure()
        {
            _isOnRequesting = false;
            
            if (_retryTimes >= MAX_RETRY_TIMES)
            {
                Log.E(LOG_TAG, $"Upload failed,reach max failed times: {MAX_RETRY_TIMES}");
                CleanupCallbacks();
                return;
            }
            
            ScheduleRetry();
        }

        
        /// <summary>
        /// 安排重试任务
        /// </summary>
        private void ScheduleRetry()
        {
            float retryDelay = (float)Math.Pow(BASE_RETRY_DELAY, _retryTimes);
            _retryTimes++;
            
            Log.I(LOG_TAG, $"Upload device ino will retry after {retryDelay} seconds, retry: {_retryTimes}");
            
            CoroutineHelper.Instance.StartDelayed(retryDelay, SendUploadRequest);
        }

        /// <summary>
        /// 清理回调
        /// </summary>
        private void CleanupCallbacks()
        {
            OnUploadSuccessEvent = null;
        }

    }
}