using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Guru;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LoadType
{
    Text,
    Bytes,
    Texture2D,
}

/// <summary>
/// CDN云控配置
/// </summary>
[Serializable]
public class CDNConfig
{
    public const string CDN_CONFIG_REMOTE_KEY = "cdn_config";

    public bool enable = true;
    public string[] replace;
    public string main;
    public string fallback;
    public int retry;
    public int timeout;

    // private static CDNConfig _config;
    
    
    /// <summary>
    /// 加载配置
    /// </summary>
    /// <returns></returns>
    public static CDNConfig Load( string remoteKey = "")
    {
        if(string.IsNullOrEmpty(remoteKey))
            remoteKey = CDN_CONFIG_REMOTE_KEY;

        CDNConfig config = null;
        // if (_config != null)  return _config;
        try
        {
            if (!FirebaseUtil.IsReady) return null;
            config = FirebaseUtil.GetRemoteConfig<CDNConfig>(remoteKey);
        }
        catch (Exception e)
        {
            Log.E(e);
            FirebaseUtil.LogException(e);
        }
        return config;
    }
}

public class CDNLoader : MonoBehaviour
{
    
    /// <summary>
    /// 默认超时时间(秒)
    /// </summary>
    private const int DEFAULT_TIMEOUT = 30;
    
    /// <summary>
    /// 默认最大重试次数
    /// </summary>
    private const int DEFAULT_MAX_RETRY = 2;
    
    /// <summary>
    /// 受保护的URL参数列表
    /// </summary>
    private static readonly string[] protectedParamKeys = new string[]
    {
        "generation"
    };
    
    /// <summary>
    /// 加载配置
    /// </summary>
    private CDNConfig _config;
    public CDNConfig Config => _config;
    private CDNConfig _defaultConfig;
    
    /// <summary>
    /// 当前是否正在加载
    /// </summary>
    private volatile bool _isOnLoading;
    
    /// <summary>
    /// 加载任务队列
    /// </summary>
    private LinkedList<LoadTask> _loadList;
    
    /// <summary>
    /// 使用备用地址
    /// </summary>
    private int _retryNum;
    private UnityWebRequest _request;
    
    private Action<int, string> onError;
    private Action<DownloadHandler> onComplete;
    private static int _instanceCount = 0;
    
    #region 初始化
    
    /// <summary>
    /// 创建 Loader (可复数创建)
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <param name="remoteKey"></param>
    /// <returns></returns>
    public static CDNLoader Create(string defaultValue = "", string remoteKey = "")
    {
        var go = new GameObject(GetInstanceName());
        DontDestroyOnLoad(go);
        var loader = go.AddComponent<CDNLoader>();

        if (!string.IsNullOrEmpty(defaultValue))
        {
            loader.SetDefaultValue(defaultValue, remoteKey);
        }

        return loader;
    }

    private static string GetInstanceName()
    {
        _instanceCount++;
        return $"{nameof(CDNLoader)} ({_instanceCount})";
    }

    /// <summary>
    /// 设置默认值
    /// </summary>
    /// <param name="defaultValue">配置默认值</param>
    /// <param name="remoteKey">注：有的项目会使用前缀， 故这里进行对应的设置</param>
    private void SetDefaultValue(string defaultValue, string remoteKey = "")
    {
        if (string.IsNullOrEmpty(remoteKey)) 
            remoteKey = CDNConfig.CDN_CONFIG_REMOTE_KEY;

        if (string.IsNullOrEmpty(defaultValue))
        {
            Debug.LogError($"[CDNLoader] set defaultValue is empty");
            return;
        }
        
        // 记录本地默认值
        _defaultConfig = JsonUtility.FromJson<CDNConfig>(defaultValue);
        // Firebase Util 添加默认值
        FirebaseUtil.AppendDefaultValue(remoteKey, defaultValue);
    }
    
    /// <summary>
    /// 资源回收
    /// </summary>
    private void OnDestroy()
    {
        StopAllCoroutines();
        
        // 清理队列
        if (_loadList != null)
        {
            foreach (var task in _loadList)
            {
                task.Dispose();
            }
            _loadList.Clear();
        }
        
        Destroy(gameObject);
    }
    
    private void Awake()
    {
        Init();
    }
    
    private void Init()
    {
        _isOnLoading = false;
        _loadList = new LinkedList<LoadTask>();
    }

    /// <summary>
    /// 获取链接地址
    /// </summary>
    /// <param name="originUrl"></param>
    /// <param name="useFallback"></param>
    /// <returns></returns>
    public string GetUrl(string originUrl,  bool useFallback = false)
    {
        CDNConfig config = GetConfig();
        if (config == null)
        {
            Debug.LogError($"[CDNLoader] GetUrl failed, config is null, return <originUrl>:{originUrl}");
            return originUrl;
        }
        
        var hostUrl = useFallback ? config.fallback : config.main;
        if (config.replace != null && config.replace.Length > 0)
        {
            foreach (var rp in config.replace)
            {
                if (originUrl.Contains(rp))
                {
                    originUrl = originUrl.Replace(rp, "");
                    break;
                }
            }
        }

        // 带有参数的URL地址过滤和拼接
        var urlParams = FixUrlParameter(originUrl);
        return $"{hostUrl}{urlParams}";
    }

    /// <summary>
    /// 过滤整理URL参数
    /// </summary>
    /// <param name="originUrl"></param>
    /// <returns></returns>
    private static string FixUrlParameter(string originUrl)
    {
        // EXAMPLE:
        // https://firebasestorage.googleapis.com/v0/b/ball-sort-dd4d0.appspot.com/o/levels%2Fspecified.json?alt=media
        
        originUrl = originUrl.Replace("%2F", "/");
        
        // 带有参数的URL地址过滤和拼接
        if (originUrl.Contains("?"))
        {
            var raw = originUrl.Split('?');
            if (raw.Length > 1)
            {
                var args = "";
                var temp = raw[1].Split('&');
                if (temp.Length > 0)
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i].Contains("="))
                        {
                            var kvp = temp[i].Split('=');
                            if (protectedParamKeys.Contains(kvp[0]))
                            {
                                args += $"{temp[i]}";
                                if (i < temp.Length - 2) args += "&";
                            }
                        }
                    }
                }

                originUrl = raw[0];
                if (args.Length > 0) originUrl += $"?{args}";
            }
        }

        return originUrl;
    }

    #endregion

    #region 队列管理

    private int TaskCount
    {
        get
        {
            if (_loadList != null) 
                return _loadList.Count;
            else
                return -1;
        }
    }

    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="task"></param>
    /// <param name="addFirst"></param>
    private void AddTask(LoadTask task, bool addFirst = false)
    {
        if (_loadList == null)
            _loadList = new LinkedList<LoadTask>();

        if(addFirst)
            _loadList.AddFirst(task);
        else
            _loadList.AddLast(task);
    }

    /// <summary>
    /// 获取任务
    /// </summary>
    /// <returns></returns>
    private LoadTask GetTask()
    {
        if (TaskCount > 0)
        {
            var node = _loadList.First;
            _loadList.RemoveFirst();
            return node.Value;
        }
        return null;
    }
    
    #endregion
    
    #region 加载接口

    /// <summary>
    /// 加载文本
    /// </summary>
    /// <param name="url"></param>
    /// <param name="type"></param>
    /// <param name="id">多条加载的时候请给 ID 值</param>
    /// <param name="onCompleteHandler"></param>
    /// <param name="onProgressHandler"></param>
    public void LoadAsset(string url, LoadType type = LoadType.Text, string id = "", 
        Action<LoadResult> onCompleteHandler = null,  Action<float> onProgressHandler = null)
    {
        if (string.IsNullOrEmpty(id)) id = url; // id 为空的情况下, 使用 URL 作为唯一标识
        AddTask(new LoadTask()
        {
            id = id,
            url = url,
            onComplete = onCompleteHandler,
            onProgress =  onProgressHandler,
            type = type,
        });
    }
    
    #endregion
    
    #region 加载队列

    /// <summary>
    /// 请务必在主线程中实例化CDNLoader
    /// </summary>
    private void Update()
    {
        // 加载中则跳过
        if (_isOnLoading) return;

        // 无任务则跳过
        if (TaskCount <= 0) return;

        // 开始下载任务
        StartCoroutine(nameof(CRLoadTask));
    }

    /// <summary>
    /// 获取云控配置
    /// </summary>
    /// <returns></returns>
    private CDNConfig GetConfig()
    {
        // 有则返回
        if(_config != null) 
            return _config;
        
        // 尝试加载
        _config = CDNConfig.Load();
        if (_config != null)
        {
            Debug.Log($"<color=#88ff00>[CDNLoader] Using onlineConfig !</color>");
            return _config;
        }


        if (_defaultConfig != null)
        {
            Debug.Log($"<color=yellow>[CDNLoader] Using defaultConfig !</color>");
            return _defaultConfig;
        }

        Debug.LogError($"[CDNLoader] Get config failed, no config exsits!!");
        return null;
    }


    /// <summary>
    /// 协程加载任务
    /// </summary>
    /// <returns></returns>
    private IEnumerator CRLoadTask()
    {
        _isOnLoading = true;
        var task = GetTask();
        if (task != null)
        {
            var config = GetConfig(); // 尝试获取 CdnConfig
            // 检查配置有效性
            if (config == null) 
            {
                var result = LoadResult.Create(task, false, "CDN configuration is missing");
                task.onComplete?.Invoke(result);
                _isOnLoading = false;
                yield break;
            }
            
            task.fixedUrl = GetUrl(task.url, task.useFallback);
            Debug.Log($"<color=#88ff00>--- start download: {task.fixedUrl}</color>");

            using (UnityWebRequest www = UnityWebRequest.Get(task.fixedUrl))
            {
                if (task.type == LoadType.Texture2D)
                    www.downloadHandler = new DownloadHandlerTexture();
                else
                    www.downloadHandler = new DownloadHandlerBuffer();

                www.timeout = config.timeout > 0 ? config.timeout : DEFAULT_TIMEOUT;
                yield return www.SendWebRequest();
                
                var result = LoadResult.Create(task);
                if (www.result == UnityWebRequest.Result.Success)
                {
                    ProcessSuccessResponse(www, task, result);
                }
                else
                {
                    ProcessFailedResponse(www, task, result, config);
                }

                _isOnLoading = false;
            }
        }
    }
    
    /// <summary>
    /// 处理成功的响应
    /// </summary>
    private void ProcessSuccessResponse(UnityWebRequest www, LoadTask task, LoadResult result)
    {
        bool success = www.downloadHandler != null;
        result.success = success;
        if (success)
        {
            switch (task.type)
            {
                case LoadType.Bytes:
                    result.data = www.downloadHandler.data;
                    break;
                case LoadType.Texture2D:
                    var handler2D = www.downloadHandler as DownloadHandlerTexture;
                    result.texture = handler2D?.texture;
                    break;
                default:
                    result.text = www.downloadHandler.text;
                    break;
            }
        }
        task.onComplete?.Invoke(result);
    }

    /// <summary>
    /// 处理失败的响应
    /// </summary>
    private void ProcessFailedResponse(UnityWebRequest www, LoadTask task, LoadResult result, CDNConfig config)
    {
        Debug.LogError($"CDNLoader load failed -> code:{www.responseCode} error:{www.error} URL:{task.fixedUrl} type:{task.type}");
        
        task.retryNum++;
        int maxRetry = config.retry > 0 ? config.retry : DEFAULT_MAX_RETRY;
        
        if (task.retryNum > maxRetry)
        {
            if (task.useFallback)
            {
                result.success = false;
                result.error = www.error;
                task.onComplete?.Invoke(result);
                task.Dispose();
            }
            else
            {
                task.retryNum = 0;
                task.useFallback = true;
                AddTask(task);
            }
        }
        else
        {
            AddTask(task);
        }
    }
    
    #endregion

    #region 单元测试
    
    /*
#if UNITY_EDITOR
    
    [MenuItem("Test/Test CDNLoader...")]
    public static void EditorTestGetURL()
    {
        string url = "https://cdn.dof.fungame.studio/PicAssets%2FAndroid%2F6_cx_0818_17?generation=1649423811796255&alt=media";
        string url1 = "https://cdn.dof.fungame.studio/PicAssets%2FAndroid%2F6_cx_0818_17?alt=media&generation=1649423811796255";
        string url2 = "https://cdn.dof.fungame.studio/PicAssets%2FAndroid%2F6_cx_0818_17?alt=media";
        string newUrl = FixUrlParameter(url1);
        Debug.Log($"---> newUrl: {newUrl}");
    }
#endif
    */

    #endregion
    
    #region 加载结果

    /// <summary>
    /// 加载结果
    /// </summary>
    public class LoadResult
    {
        public string id;
        public LoadType type;
        public bool success = false;
        public string url;
        public string error;
        public string text = "";
        public byte[] data = null;
        public Texture2D texture = null;
        
        internal static LoadResult Create(LoadTask task, bool success = false, string error = "")
        {
            var result = new LoadResult()
            {
                id = task.id,
                type = task.type,
                success = success,
                url = task.fixedUrl,
                error = error
            };
            return result;
        }
        
        
    }

    #endregion
}

/// <summary>
/// 加载任务
/// </summary>
[Serializable]
internal class LoadTask
{
    public string url;
    public string fixedUrl;
    public string id;
    public Action<CDNLoader.LoadResult> onComplete;
    public Action<float> onProgress;
    public bool useFallback = false;
    public int retryNum = 0;
    public LoadType type;
    public int piority = 100;

    public void Dispose()
    {
        onComplete = null;
        onProgress = null;
    }
}

