
namespace Guru
{
    using System;
    
    /// <summary>
    /// LT 属性上报器
    /// </summary>
    public partial class GuruSDK: ILTPropertyReporter
    {


        /// <summary>
        /// 初始化 LT Property
        /// </summary>
        private void InitLTProperty()
        {
#if UNITY_EDITOR
            // Editor 测试用代码
            _ = new EditorLTPropertyManager(this);
            return;
#endif
            
            _ = new UserLTPropertyManager(Model, this);
        }

        /// <summary>
        /// 上报用户属性 LT 
        /// </summary>
        /// <param name="lt"></param>
        public void ReportUserLT(int lt)
        {
            SetUserProperty(Consts.PropertyLT, $"{lt}");
        }


    }
    
    
    
    
    
}