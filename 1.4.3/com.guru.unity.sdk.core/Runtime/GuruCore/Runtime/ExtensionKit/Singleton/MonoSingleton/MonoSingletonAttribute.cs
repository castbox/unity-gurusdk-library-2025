using System;

namespace Guru
{
    /// <summary>
    /// MonoBehaviour单例属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class MonoSingletonAttribute : Attribute
    {
        public readonly EMonoSingletonType[] SingletonCreateTypes;
        public readonly bool DestroyOnLoad;
        public readonly string ResourcesLoadPath;
        public readonly bool AllowSetInstance;

        public MonoSingletonAttribute(EMonoSingletonType singletonCreateType, bool destroyInstanceOnSceneLoad = true,
            string resourcesPath = "", bool allowSetInstance = false)
        {
            SingletonCreateTypes = new[] {singletonCreateType};
            DestroyOnLoad = destroyInstanceOnSceneLoad;
            ResourcesLoadPath = resourcesPath;
            AllowSetInstance = allowSetInstance;
        }

        public MonoSingletonAttribute(EMonoSingletonType[] singletonCreateTypes, bool destroyInstanceOnSceneLoad = true,
            string resourcesPath = "", bool allowSetInstance = false)
        {
            SingletonCreateTypes = singletonCreateTypes;
            DestroyOnLoad = destroyInstanceOnSceneLoad;
            ResourcesLoadPath = resourcesPath;
            AllowSetInstance = allowSetInstance;
        }
    }
}