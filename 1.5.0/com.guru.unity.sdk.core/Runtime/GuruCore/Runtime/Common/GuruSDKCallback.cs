namespace Guru
{
    using UnityEngine;
    using System;
    
    /// <summary>
    /// SDK回调实体
    /// </summary>
    public class GuruSDKCallback: MonoBehaviour
    {
        public const string ObjectName = "GuruCallback";
        public const string MethodName = nameof(OnCallback);
        
        
        private event Action<string> msgCallback;
        
        private static GuruSDKCallback _instance;
        public static GuruSDKCallback Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Create();
                }
                return _instance;    
            }
        }
        
        /// <summary>
        /// 创建对象
        /// </summary>
        /// <returns></returns>
        private static GuruSDKCallback Create()
        {
            var go = new GameObject();
            go.name = ObjectName;
            DontDestroyOnLoad(go);
            var ins = go.AddComponent<GuruSDKCallback>();
            return ins;
        }
        
        /// <summary>
        /// External 回调参数
        /// </summary>
        /// <param name="message"></param>
        public void OnCallback(string message)
        {
            msgCallback?.Invoke(message);
        }
        
        /// <summary>
        /// 添加回调
        /// </summary>
        /// <param name="callback"></param>
        public static void AddCallback(Action<string> callback)
        {
            Instance.msgCallback += callback;
        }
        
        /// <summary>
        /// 添加回调
        /// </summary>
        /// <param name="callback"></param>
        public static void RemoveCallback(Action<string> callback)
        {
            Instance.msgCallback -= callback;
        }
        
    }
}