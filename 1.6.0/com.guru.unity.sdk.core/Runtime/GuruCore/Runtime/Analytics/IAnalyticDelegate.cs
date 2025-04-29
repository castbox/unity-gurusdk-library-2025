using System.Collections.Generic;

namespace Guru
{
    public interface IAnalyticDelegate
    {
        /// <summary>
        /// 自定义的事件驱动器
        /// 方便项目组自己扩展和实现逻辑
        /// </summary>
        public IReadOnlyList<AbstractEventDriver> CustomEventDrivers { get; }
        
    }
}