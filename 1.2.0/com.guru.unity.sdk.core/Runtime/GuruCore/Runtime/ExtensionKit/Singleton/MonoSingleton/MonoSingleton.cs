using System;
using UnityEngine;

namespace Guru
{
    /// <summary>
    /// Unity MonoBehaviour单例基类
    /// use eg:
    ///     [UnitySingleton(UnitySingleton.SingletonType.LoadedFromResources, false, "test")] 
    ///     class A : MonoSingleton<A> { }
    /// 注意：必须注明Unity单例的属性
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static bool InstanceExists => _instance != null;

        public static T Instance
        {
            get
            {
                TouchInstance();
                return _instance;
            }
            set
            {
                var attribute = Attribute.GetCustomAttribute(typeof(T), typeof(MonoSingletonAttribute)) as MonoSingletonAttribute;
                if (attribute == null)
                {
                    Debug.LogError("Cannot find MonoSingleton attribute on " + typeof(T).Name);
                    return;
                }

                if (attribute.AllowSetInstance)
                {
                    _instance = value;
                }
                else
                {
                    Debug.LogError(typeof(T).Name +
                        " is not allowed to set instances.  Please set the allowSetInstace flag to true to enable this feature.");
                }
            }
        }

        public static void DestroyInstance(bool destroyGameObject = true)
        {
            if (InstanceExists)
            {
                if (destroyGameObject)
                {
                    Destroy(_instance.gameObject);
                }
                else
                {
                    Destroy(_instance);
                }

                _instance = null;
            }
        }

        /// <summary>
        /// Called when this object is created.
        /// Children should call this base method when overriding.
        /// </summary>
        protected virtual void Awake()
        {
            if (InstanceExists && _instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 单例提供的一个空接口，用来初始化单例
        /// </summary>
        public void Touch()
        {
        }

        /// <summary>
        /// Ensures that an instance of this singleton is generated
        /// </summary>
        private static void TouchInstance()
        {
            if (!InstanceExists)
                Generate();
        }

        /// <summary>
        /// Generates this singleton
        /// </summary>
        private static void Generate()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(T), typeof(MonoSingletonAttribute)) as MonoSingletonAttribute;
            if (attribute == null)
            {
                Debug.LogError("Cannot find MonoSingleton attribute on " + typeof(T).Name);
                return;
            }

            for (int i = 0; i < attribute.SingletonCreateTypes.Length; i++)
            {
                if (TryGenerateInstance(attribute.SingletonCreateTypes[i], attribute.DestroyOnLoad,
                    attribute.ResourcesLoadPath, i == attribute.SingletonCreateTypes.Length - 1))
                    break;
            }
        }

        /// <summary>
        /// Attempts to generate a singleton with the given parameters
        /// </summary>
        /// <param name="singletonType">创建单例方式</param>
        /// <param name="destroyOnLoad">关卡切换是否销毁</param>
        /// <param name="resourcesLoadPath">resource加载路径</param>
        /// <param name="warn">最后一种创建方式，没有创建成功显示报错</param>
        /// <returns></returns>
        private static bool TryGenerateInstance(EMonoSingletonType singletonType, bool destroyOnLoad,
            string resourcesLoadPath, bool warn)
        {
            if (singletonType == EMonoSingletonType.ExitsInScene)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    if (warn)
                    {
                        Debug.LogError("Cannot find an object with a " + typeof(T).Name +
                                       " .  Please add one to the scene.");
                    }

                    return false;
                }
            }
            else if (singletonType == EMonoSingletonType.LoadedFromResources)
            {
                if (string.IsNullOrEmpty(resourcesLoadPath))
                {
                    if (warn)
                    {
                        Debug.LogError(
                            "MonoSingletonAttribute.resourcesLoadPath is not a valid Resources location in " +
                            typeof(T).Name);
                    }

                    return false;
                }

                T pref = Resources.Load<T>(resourcesLoadPath);
                if (pref == null)
                {
                    if (warn)
                    {
                        Debug.LogError("Failed to load prefab with " + typeof(T).Name +
                                       " component attached to it from folder Resources/" + resourcesLoadPath +
                                       ".  Please add a prefab with the component to that location, or update the location.");
                    }

                    return false;
                }

                _instance = Instantiate(pref);
                if (_instance == null)
                {
                    if (warn)
                    {
                        Debug.LogError("Failed to create instance of prefab " + pref + " with component " +
                                       typeof(T).Name + ".  Please check your memory constraints");
                    }

                    return false;
                }

                _instance.name = "(Singleton)" + typeof(T).Name;
            }
            else if (singletonType == EMonoSingletonType.CreateOnNewGameObject)
            {
                GameObject go = new GameObject("(Singleton)" + typeof(T).Name);
                if (go == null)
                {
                    if (warn)
                    {
                        Debug.LogError("Failed to create gameobject for instance of " + typeof(T).Name +
                                       ".  Please check your memory constraints.");
                    }

                    return false;
                }

                _instance = go.AddComponent<T>();
                if (_instance == null)
                {
                    if (warn)
                    {
                        Debug.LogError("Failed to add component of " + typeof(T).Name +
                                       " to new gameobject.  Please check your memory constraints.");
                    }

                    Destroy(go);
                    return false;
                }
            }

            if (!destroyOnLoad)
            {
                DontDestroyOnLoad(_instance.gameObject);
            }

            return true;
        }
    }
}