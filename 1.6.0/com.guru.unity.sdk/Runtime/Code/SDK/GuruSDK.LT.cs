namespace Guru
{
    using System;
    
    /// <summary>
    /// LT 属性上报器
    /// </summary>
    public partial class GuruSDK: ILTPropertyReporter
    {
        private ILTPropertyManager _ltManager;

        /// <summary>
        /// 初始化 LT Property
        /// </summary>
        private void InitLTProperty()
        {
            _ltManager = new UserLTPropertyManager(Model, this);
            
#if UNITY_EDITOR
            // Editor 测试用代码
            _ltManager = new EditorLTPropertyManager(this);
#endif

            // 游戏暂停回调
            if(_ltManager != null)
                Callbacks.App.OnAppPaused += _ltManager.OnApplicationPause;
        }

        /// <summary>
        /// 上报用户属性 LT 
        /// </summary>
        /// <param name="lt"></param>
        public void ReportUserLT(int lt)
        {
            SetUserProperty(Consts.PropertyLT, $"{lt}");
        }

        /// <summary>
        /// 开启 Debug 模式
        /// </summary>
        /// <param name="value"></param>
        private void SetLTDebugMode(bool value)
        {
            _ltManager?.SetDebugMode(value);
        }

    }
    
    
    
    
    
}