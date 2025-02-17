using System;

namespace Guru
{
    public class Singleton<T> where T : class, new()
    {
        private static readonly Lazy<T> _lazy = new Lazy<T>(() => new T());
        public static T Instance => _lazy.Value;
        
        protected Singleton()
        {
            Init();
        }

        /// <summary>
        /// 仅仅用来初始化单例成员变量，不要用来做初始化操作【多次在Init里写其他操作又调回本单例，导致循环调用引发程序崩溃】
        /// 建议每个单例初始化操作另写函数
        /// </summary>
        protected virtual void Init()
        {
        }
        
        /// <summary>
        /// 提供一个创建单例接口
        /// </summary>
        public void Touch()
        {
        }
    }
}