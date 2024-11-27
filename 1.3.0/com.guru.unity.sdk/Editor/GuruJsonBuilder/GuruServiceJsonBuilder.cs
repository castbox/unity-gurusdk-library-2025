#if GURU_SDK_DEV

namespace Guru.Editor
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Networking;
    using System.Collections.Generic;
    using System.Linq;
    
    public class GuruServiceJsonBuilder: EditorWindow
    {
                
        // Tool Version
        public const string Version = "1.1.0";
        
        const string LocalProjectSettingsList = "guru-project-settings.cfg";

        private const string OnlineProjectDataUrl =
            "https://docs.google.com/spreadsheets/d/e/2PACX-1vTVoTf7RcDWX3I8N2UiOq9hOlkaB2s1PzrUdOfSNomrWUO0_aAG2EZnljH_KZ5CLtAZNqPH6a7UxTEt/pub?gid=914170746&single=true&output=tsv";
        
        private const string K_APP_SETTINGS = "app_settings";
        private const string K_ADJUST_SETTINGS = "adjust_settings";
        private const string K_FB_SETTINGS = "fb_settings";
        private const string K_AD_SETTINGS = "ad_settings";
        private const string K_IAP_SETTINGS = "iap_settings";
        private const char K_SPLITTER_TAB = '\t';
        private const char K_SPLITTER_COMMA = ',';
        
        private const string NoSelectionName = "------";
        private const string STATE_IDLE = "s_idle";
        private const string STATE_PROGRESS = "s_progress";
        private const string STATE_BUILDING = "s_building";
        private const string STATE_SUCCESS = "s_success";
        private const int NETWORK_RETRY_TIME = 0;

        private static GuruServiceJsonBuilder _instance;
        private List<string> _projectNames;
        private string[] _activeNameList;
        private string _searchKeyword = "";
        private int _selectedProjectIndex = 0;
        private static Dictionary<string, string> _publishLinks;
        private string _labelName = "";
        private int _loadRetryTimes = 0;
        
        private static Dictionary<string, string> PublishLinks
        {
            get
            {
                if (_publishLinks == null)
                {
                    _publishLinks = LoadProjectSettingsCfg();
                    Debug.LogError("拉取网络配置失败，改为加载本地配置");
                }
                return _publishLinks;
            }
        }

        #region Link & Keys

        private const string TSVLink = "https://docs.google.com/spreadsheets/d/e/{0}/pub?gid=0&single=true&output=tsv";
        

        #endregion
        
        #region Export JSON
        
        /// <summary>
        /// 从 TSV 文件进行转化
        /// </summary>
        /// <param name="tsv"></param>
        /// <param name="savePath"></param>
        public static void ConvertFromTSV(string tsv, string savePath = "")
        {
            if (string.IsNullOrEmpty(tsv))
            {
                EditorUtility.DisplayDialog("空文件!", $"文件格式错误!\n{tsv}", "OK");
                return;
            }
            
            var guru_service = EditorGuruServiceIO.CreateEmpty();

            var lines = tsv.Split('\n');
            string line = "";
            for (int index = 0; index < lines.Length; index++)
            {
                line = lines[index];
                if (!IsInvalidLine(line))
                {
                    //---------------- app_settings ----------------
                    if (line.StartsWith(K_APP_SETTINGS))
                    {
                        index++;
                        while (!line.StartsWith(K_ADJUST_SETTINGS))
                        {
                            line = lines[index];
                            FillAppSettings(guru_service, line);
                            index++;
                            line = lines[index];
                        }
                    }
                    //---------------- adjust_settings ----------------
                    if (line.StartsWith(K_ADJUST_SETTINGS))
                    {
                        index++;
                        FillAdjustSettings(guru_service, lines, ref index);
                    }
                    //---------------- fb_settings ----------------
                    if (line.StartsWith(K_FB_SETTINGS))
                    {
                        index++;
                        FillFacebookSettings(guru_service, lines, ref index);
                    }
                    //---------------- ad_settings ----------------
                    if (line.StartsWith(K_AD_SETTINGS))
                    {
                        index++;
                        FillAdSettings(guru_service, lines, ref index);
                    }
                    //---------------- iap_settings ----------------
                    if (line.StartsWith(K_IAP_SETTINGS))
                    {
                        index++;
                        FillProducts(guru_service, lines, ref index);
                    }
                }
                
            }

            guru_service.version = GetFileVersionByDate();
            
            if (string.IsNullOrEmpty(savePath))
            {
                var dir = Path.GetFullPath($"{Application.dataPath}/../output");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                savePath = $"{dir}/guru-services-{guru_service.app_settings.app_id.ToLower()}.json";
            }

            var arr = savePath.Split('/');
            var fileName = arr[arr.Length - 1];

            EditorGuruServiceIO.SourceServiceFilePath = savePath;
            EditorGuruServiceIO.SaveConfig(guru_service, savePath);

            // if (EditorUtility.DisplayDialog("CONVERT SUCCESS!", $"Export Json File\n{fileName}\nto:\n{savePath}", "OK"))
            // {
            //     GuruEditorHelper.OpenPath(Directory.GetParent(savePath)?.FullName ?? Application.dataPath);
            // }

            ResetGuruServiceInProject(savePath);
        }


        public static void ConvertFromTsvFile(string tsvPath, string savePath = "")
        {
            if (!File.Exists(tsvPath))
            {
                EditorUtility.DisplayDialog("FILE NOT FOUND!", $"File not exist:\n{tsvPath}", "OK");
                return;
            }
            
            var tsvString = File.ReadAllText(tsvPath);
            if (!string.IsNullOrEmpty(tsvString))
            {
                ConvertFromTSV(tsvString, savePath);
            }
        }


        /// <summary>
        /// AppSettings 填充
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="line"></param>
        private static void FillAppSettings(GuruServicesConfig settings, string line)
        {
            // 对于空行和空值直接跳过
            if (IsInvalidLine(line)) return;

            string value = "";
            if (settings.app_settings == null) settings.app_settings = new GuruAppSettings();
            if (settings.parameters == null) settings.parameters = new GuruParameters();

            //------------------- AppSettings -------------------------------
            // 拾取值和注入
            if (GetValue(line, "app_id", out value))
            {
                settings.app_settings.app_id = value;
            }
            else if (GetValue(line, "product_name", out value))
            {
                settings.app_settings.product_name = value;
            }
            else if (GetValue(line, "bundle_id", out value))
            {
                settings.app_settings.bundle_id = value;
            }
            else if (GetValue(line, "support_email", out value))
            {
                settings.app_settings.support_email = value;
            }
            else if (GetValue(line, "privacy_url", out value))
            {
                settings.app_settings.privacy_url = value;
            }
            else if (GetValue(line, "terms_url", out value))
            {
                settings.app_settings.terms_url = value;
            }
            else if (GetValue(line, "android_store", out value))
            {
                settings.app_settings.android_store = value;
            }
            else if (GetValue(line, "ios_store", out value))
            {
                settings.app_settings.ios_store = value;
            }
            else if (GetValue(line, "enable_firebase", out value))
            {
                settings.app_settings.enable_firebase = GetBool(value);
            }
            else if (GetValue(line, "enable_facebook", out value))
            {
                settings.app_settings.enable_facebook = GetBool(value);
            }
            else if (GetValue(line, "enable_adjust", out value))
            {
                settings.app_settings.enable_adjust = GetBool(value);
            }
            else if (GetValue(line, "enable_iap", out value))
            {
                settings.app_settings.enable_iap = GetBool(value);
            }
            else if (GetValue(line, "custom_keystore", out value))
            {
                settings.app_settings.custom_keystore = GetBool(value);
            }
            //------------------- Parameters -------------------------------
            else if (GetValue(line, "tch_fb_mode", out value))
            {
                settings.parameters.tch_fb_mode = GetInt(value);
            }
            else if (GetValue(line, "using_uuid", out value))
            {
                settings.parameters.using_uuid = GetBool(value);
            }
            else if (GetValue(line, "cdn_host", out value))
            {
                settings.parameters.cdn_host = value;
            }
            else if (GetValue(line, "enable_errorlog", out value))
            {
                settings.parameters.enable_errorlog = GetBool(value);
            }
            else if (GetValue(line, "level_end_success_num", out value))
            {
                settings.parameters.level_end_success_num = GetInt(value);
            }
            else if (GetArray(line, "url_schema", out var list))
            {
                settings.parameters.url_schema = list; // 输入URLSchema 
            }
        }

        /// <summary>
        /// AdjustSettings 填充
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="line"></param>
        private static void FillAdjustSettings(GuruServicesConfig settings, string[] lines, ref int index)
        {
            if(settings.adjust_settings == null) settings.adjust_settings = new GuruAdjustSettings();
            string[] list = null;
            string line = lines[index];
            bool pass = false;
            List<string> events = new List<string>(40);
            
            while (!lines[index].StartsWith(K_FB_SETTINGS))
            {
                line = lines[index];
                if (!IsInvalidLine(line))
                {
                    if (line.StartsWith("app_token"))
                    {
                        list = GetStringArray(line, 1, 2);
                        settings.adjust_settings.app_token = list;
                    }
                    else
                    {
                        list = GetStringArray(line, 0, 3);
                        pass = list != null && !string.IsNullOrEmpty(list[0]) 
                                            && (!string.IsNullOrEmpty(list[1]) || !string.IsNullOrEmpty(list[2]));
                        if( pass) events.Add(string.Join(",", list));
                    }
                }
                index++;
            }

            settings.adjust_settings.events = events.ToArray();
            index--;
        }

        private static long GetFileVersionByDate()
        {
            var startDt = new DateTime(1970,1,1,0,0,0);
            return (long) (DateTime.UtcNow.Ticks - startDt.Ticks) / 10000;
        }


        private static void FillFacebookSettings(GuruServicesConfig settings, string[] lines, ref int index)
        {
            string value = "";
            if(settings.fb_settings == null) settings.fb_settings = new GuruFbSettings();
            var line = "";

            while (!lines[index].StartsWith(K_AD_SETTINGS))
            {
                line = lines[index];
                if (!IsInvalidLine(line))
                {
                    // 拾取值和注入
                    if (GetValue(line, "fb_app_id", out value))
                    {
                        settings.fb_settings.fb_app_id = value;
                    }
                    else if (GetValue(line, "fb_client_token", out value))
                    {
                        settings.fb_settings.fb_client_token = value;
                    }
                }
                index++;
            }
            
            index--;
        }
        
        private static void FillAdSettings(GuruServicesConfig settings, string[] lines, ref int index)
        {
            string value = "";
            if(settings.ad_settings == null) settings.ad_settings = new GuruAdSettings();
            
            
            var line = lines[index];
            // SDK Key
           

            string[] max_ids_android = new string[3];
            string[] max_ids_ios = new string[3];
            string[] amazon_ids_android = new string[4];
            string[] amazon_ids_ios = new string[4];
            string[] pubmatic_ids_android = new string[3];
            string[] pubmatic_ids_ios = new string[3];
            string[] moloco_ids_android = new string[3];
            string[] moloco_ids_ios = new string[3];
            string[] tradplus_ids_android = new string[3];
            string[] tradplus_ids_ios = new string[3];
            
            
            //------- 开始记录广告配置;
            string[] arr;
            while (!lines[index].StartsWith(K_IAP_SETTINGS))
            {
                line = lines[index];
                
                if (GetValue(line, "sdk_key", out value))
                {
                    settings.ad_settings.sdk_key = value;
                }
                else if (line.StartsWith("admob_app_id"))
                {
                    settings.ad_settings.admob_app_id = GetStringArray(line, 1, 2);
                }
                else if (line.StartsWith("max_bads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    max_ids_android[0] = arr[0];
                    max_ids_ios[0] = arr[1];
                }
                else if (line.StartsWith("max_iads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    max_ids_android[1] = arr[0];
                    max_ids_ios[1] = arr[1];
                }
                else if (line.StartsWith("max_rads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    max_ids_android[2] = arr[0];
                    max_ids_ios[2] = arr[1];
                }
                else if (line.StartsWith("amazon_app_id"))
                {
                    arr = GetStringArray(line, 1, 2);
                    amazon_ids_android[0] = arr[0];
                    amazon_ids_ios[0] = arr[1];
                }
                else if (line.StartsWith("amazon_bads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    amazon_ids_android[1] = arr[0];
                    amazon_ids_ios[1] = arr[1];
                }
                else if (line.StartsWith("amazon_iads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    amazon_ids_android[2] = arr[0];
                    amazon_ids_ios[2] = arr[1];
                }
                else if (line.StartsWith("amazon_rads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    amazon_ids_android[3] = arr[0];
                    amazon_ids_ios[3] = arr[1];
                }
                else if (line.StartsWith("pubmatic_bads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    pubmatic_ids_android[0] = arr[0];
                    pubmatic_ids_ios[0] = arr[1];
                }
                else if (line.StartsWith("pubmatic_iads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    pubmatic_ids_android[1] = arr[0];
                    pubmatic_ids_ios[1] = arr[1];
                }
                else if (line.StartsWith("pubmatic_rads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    pubmatic_ids_android[2] = arr[0];
                    pubmatic_ids_ios[2] = arr[1];
                }
                else if (line.StartsWith("moloco_bads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    moloco_ids_android[0] = arr[0];
                    moloco_ids_ios[0] = arr[1];
                }
                else if (line.StartsWith("moloco_iads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    moloco_ids_android[1] = arr[0];
                    moloco_ids_ios[1] = arr[1];
                }
                else if (line.StartsWith("moloco_rads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    moloco_ids_android[2] = arr[0];
                    moloco_ids_ios[2] = arr[1];
                }
                else if (line.StartsWith("tradplus_bads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    tradplus_ids_android[0] = arr[0];
                    tradplus_ids_ios[0] = arr[1];
                }
                else if (line.StartsWith("tradplus_iads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    tradplus_ids_android[1] = arr[0];
                    tradplus_ids_ios[1] = arr[1];
                }
                else if (line.StartsWith("tradplus_rads"))
                {
                    arr = GetStringArray(line, 1, 2);
                    tradplus_ids_android[2] = arr[0];
                    tradplus_ids_ios[2] = arr[1];
                }
                index++;
            }
            
            //-------- Fill all data -----------
            settings.ad_settings.max_ids_android = max_ids_android;
            settings.ad_settings.max_ids_ios = max_ids_ios;
            settings.ad_settings.amazon_ids_android = amazon_ids_android;
            settings.ad_settings.amazon_ids_ios = amazon_ids_ios;
            settings.ad_settings.pubmatic_ids_android = pubmatic_ids_android;
            settings.ad_settings.pubmatic_ids_ios = pubmatic_ids_ios;
            settings.ad_settings.moloco_ids_android = moloco_ids_android;
            settings.ad_settings.moloco_ids_ios = moloco_ids_ios;
            settings.ad_settings.tradplus_ids_android = tradplus_ids_android;
            settings.ad_settings.tradplus_ids_ios = tradplus_ids_ios;
            
            index--;
        }
        
        private static void FillProducts(GuruServicesConfig settings, string[] lines, ref int index)
        {
            string line = "";
            List<string> iaps = new List<string>(30);
            
            string[] arr;
            while (index < lines.Length)
            {
                line = lines[index];
                if(IsInvalidLine(line)) continue;
                arr = GetStringArray(line, 0, 7);
                if(string.IsNullOrEmpty(arr[5])) arr[5] = "Store";
                if(string.IsNullOrEmpty(arr[6])) arr[6] = "0";
                iaps.Add(string.Join(",", arr).Replace("\r", ""));
                index++;
            }
            settings.products = iaps.ToArray();
            index--;
        }
        

        #endregion

        #region Utils
        
        private static bool GetBool(string value)
        {
            return value.ToLower() == "true" || value == "1";
        }
        
        private static int GetInt(string value)
        {
            int val = 0;
            int.TryParse(value, out val);
            return val;
        }
        
        private static double GetDouble(string value)
        {
            double val = 0;
            double.TryParse(value, out val);
            return val;
        }


        private static bool IsInvalidLine(string line)
        {
            return string.IsNullOrEmpty(line) || line.StartsWith(K_SPLITTER_TAB.ToString());
        }


        private static bool GetValue(string line, string key, out string value)
        {
            value = ""; // default  
            if (line.StartsWith(key))
            {
                value = line.Split(K_SPLITTER_TAB)[1];
                if (string.IsNullOrEmpty(value)) value = "empty";
                return true;
            }
            return false;
        }
        
        private static bool GetArray(string line, string key, out string[] stringArray)
        {
            stringArray = null; // default  
            if (line.StartsWith(key))
            {
                var rawArr = line.Split(K_SPLITTER_TAB);
                if (rawArr == null || rawArr.Length == 0) return false;
                var list = new List<string>(rawArr.Length);
                for (int i = 0; i < rawArr.Length; i++)
                {
                    if (string.IsNullOrEmpty(rawArr[i]))
                    {
                        stringArray = list.ToArray();
                        return list.Count > 0;
                    }
                 
                    // Skip the key pos
                    if(i > 0) list.Add(rawArr[i]);
                }
            }
            return false;
        }

        private static string[] GetStringArray(string line, int startIndex = 0, int length = 0, char spliter = K_SPLITTER_TAB)
        {
            if (IsInvalidLine(line)) return null;
            
            var raw = line.Split(spliter);
            if (length == 0) length = raw.Length;
            
            var a = new List<string>(length);
            for (int i = startIndex; i < length + startIndex; i++)
            {
                if (i < raw.Length)
                {
                    a.Add(raw[i]);
                }
                else
                {
                    a.Add("");
                }
            }
            return a.ToArray();
        }
        #endregion
        
        #region Menu Items

#if GURU_SDK_DEV
        [MenuItem("Tools/Export Guru Service", false, 0 )]
#endif
        private static void ExportJsonFile()
        {
            string saveDir = Path.GetFullPath($"{Application.dataPath}/../output");
            string saveFile = Path.Combine(saveDir,$"guru-services____{DateTime.Now:yyyy-M-d-HH-mm}.json");
            
            if(!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
            
            string searchPath = "~/Downloads/";
            string tsvPath = EditorUtility.OpenFilePanel("Load Guru Service TSV", searchPath, ".tsv");
            if (!string.IsNullOrEmpty(tsvPath))
            {
                ConvertFromTsvFile(tsvPath, saveFile);
            }
        }
        

        [MenuItem("Guru/Guru-Service Json Builder...", false, 0)]
        private static void OpenWindow()
        {
            if(_instance != null ) _instance.Close();
            _instance = GetWindow<GuruServiceJsonBuilder>();
            _instance.Show();
        }

        #endregion

        #region LocalSettings

        private static string GetRelativeDir()
        {
            var guids = AssetDatabase.FindAssets(nameof(GuruServiceJsonBuilder));
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var rpath = Directory.GetParent(path).FullName;
                return rpath;
            }
            return Path.GetFullPath($"Assets/../Packages/Editor/GuruJsonBuilder");
        }

        private static Dictionary<string, string> LoadProjectSettingsCfg()
        {
            var cfgPath = $"{GetRelativeDir()}/{LocalProjectSettingsList}";
            if (File.Exists(cfgPath))
            {
                var lines = File.ReadAllLines(cfgPath);
                int len = lines?.Length ?? -1;
                if (len > 0)
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>(lines.Length);
                    int i = 0;
                    string[] raw;
                    string line, key, value;
                    while (i < len)
                    {
                        line = lines[i];
                        if(string.IsNullOrEmpty(line)) continue;
                        raw = lines[i].Split(',');
                        value = "";
                        key = raw[0];
                        if(string.IsNullOrEmpty(key)) continue; 
                        if(raw.Length > 1) value = raw[1];
                        dict[key] = value;
                        i++;
                    }
                    return dict;
                }
            }
            return null;
        }

        #endregion
        
        #region Window

        private void Awake()
        {
            // Debug.Log($"------- Awake -------");
            this.titleContent = new GUIContent("Guru Service Builder");
        }
        

        private void OnEnable()
        {
            FetchOnlineProjectList();
        }
        
        private void OnGUI()
        {
            GUI_Title();
            switch (_state)
            {
                case STATE_IDLE:
                    GUI_Projects();
                    break;
                case STATE_PROGRESS:
                    GUI_OnProgress();
                    break;
            }
        }


        #endregion

        #region GUI


        private string _state = "";

        private void SetProgressState(string labelName = "")
        {
            _labelName = labelName;
            _state = STATE_PROGRESS;
        }
        
        private void GUI_Title()
        {
            var s = new GUIStyle("box")
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                stretchWidth = true,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(4, 4, 4, 4),
            };
            GUILayout.Label("Guru-Service Json Builder", s, GUILayout.Height(60));
            s.fontSize = 12;
            GUILayout.Label($"Version: {Version}", s);
            GUILayout.Space(4);
        }
        
        private void GUI_OnProgress()
        {
            var s = new GUIStyle("box")
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                fixedHeight = 60,
                padding = new RectOffset(4, 4, 4, 4),
            };

            GUILayout.Label($"{_labelName}", s);
            GUILayout.Space(10);
        }
        

        private void GUI_Projects()
        {
            // 搜索框
            GUI_SearchBox();
            
            // Project List Arear
            EditorGUILayout.BeginHorizontal(new GUIStyle("box"));
            EditorGUILayout.LabelField("项目列表", GUILayout.Width(60));
            _selectedProjectIndex = EditorGUILayout.Popup(_selectedProjectIndex, _activeNameList);
            GUILayout.Space(4);
            GUI_Button("刷新", FetchOnlineProjectList, Color.cyan);
            EditorGUILayout.EndHorizontal();

            if (_selectedProjectIndex >= _activeNameList.Length)
            {
                _selectedProjectIndex = 0;
            }

            var projectId = _activeNameList[_selectedProjectIndex];

            if (projectId == NoSelectionName)
            {
                GUILayout.Space(5);                
                EditorGUILayout.HelpBox("请选择一个可用项目", MessageType.Info);
                return;
            }

            GUILayout.Space(5);
            GUI_Button($"更新【{projectId}】配置", () =>
            {
                
                if (projectId == NoSelectionName)
                {
                    ShowDialog("选择错误", "请选择一个存在的项目");
                }
                else
                {
                    DownloadServiceTsvAndBuild(_selectedProjectIndex, (success, txt) =>
                    {
                        if (success)
                        {
                            ConvertFromTSV(txt);
                        }
                        else
                        {
                            ShowDialog("网络错误", txt);
                        }
                        _state = STATE_IDLE;
                    });

                    SetProgressState("配置生成中...");
                }
            }, Color.green, 60);
        }


        private void ShowDialog(string title, string content, string okName = "OK", Action onOKCallback = null,
            string cancelName = "", Action onCancelCallback = null)
        {
            if (EditorUtility.DisplayDialog(title, content, okName, cancelName))
            {
                onOKCallback?.Invoke();
            }
            else
            {
                onCancelCallback?.Invoke();
            }
        }


        /// <summary>
        /// 搜索框
        /// </summary>
        private void GUI_SearchBox()
        {
            GUILayout.BeginVertical(new GUIStyle("box"));
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("搜索项目", GUILayout.Width(60));
            
            GUI.SetNextControlName("search_box");
            _searchKeyword = EditorGUILayout.TextField(_searchKeyword).ToLower();

            var list = new List<string>();

            if (!string.IsNullOrEmpty(_searchKeyword))
            {
                foreach (var p in _projectNames)
                {
                    if (p.ToLower().Contains(_searchKeyword))
                    {
                        list.Add(p);
                    }
                }
            }
            
            GUI_Button("清除", () =>
            {
                _searchKeyword = "";
            }, Color.yellow);
            
            EditorGUILayout.EndHorizontal();
            
            
            
            
            _activeNameList = _projectNames.ToArray();
            if (list.Count == 0 && !string.IsNullOrEmpty(_searchKeyword))
            {
                // EditorGUILayout.HelpBox("无匹配结果", MessageType.Warning);
                EditorGUILayout.LabelField($"无匹配结果");
            }
            else if(list.Count > 0)
            {
                _activeNameList = list.ToArray();
                EditorGUILayout.LabelField($"已找到  {list.Count} 个项目");
            }
            
            EditorGUILayout.EndVertical();
            
        }



        #endregion
        
        #region Utils

        private static void GUI_Button(string label, Action onClick, Color color = default, int height = 0, int width = 0)
        {
            bool setColor = color != default;
            Color _c = Color.clear;
            if (setColor)
            {
                _c = GUI.color;
                GUI.color = color;
            }

            List<GUILayoutOption> opts = new List<GUILayoutOption>();
            if(height > 0) opts.Add(GUILayout.Height(height));  
            if(width > 0) opts.Add(GUILayout.Width(width));
            
            
            if (GUILayout.Button(label, (opts.Count > 0 ? opts.ToArray() : null)))
            {
                onClick?.Invoke();
            }

            if (setColor)
            {
                GUI.color = _c;
            }
        }

        #endregion
        
        #region Networking

        //--------------------- Online Project List ----------------------------
        
        /// <summary>
        /// 获取在线项目列表
        /// </summary>
        private void FetchOnlineProjectList()
        {
            // Debug.Log($"------- OnEnable -------");
            SetProgressState("\u2708 获取线上项目列表\n加载中...");
            LoadOnlineProjectSettings(settings =>
            {
                _publishLinks = settings;
                _projectNames = new List<string>(20);
                string[] names = settings.Keys.ToArray();
                string name = "";
                for(int i = 0; i < names.Length; i++)
                {
                    name = names[i];
                    if (name == "Default")
                    {
                        _projectNames.Insert(0, NoSelectionName);
                    }
                    else
                    {
                        _projectNames.Add(name);
                    }
                }

                _selectedProjectIndex = 0;
                _state = STATE_IDLE;
                this.Repaint();
                // this.Focus();
            });
        }
        
        /// <summary>
        /// 加载在线项目配置
        /// </summary>
        private void LoadOnlineProjectSettings(Action<Dictionary<string, string>> onLoaded)
        {
            var www = UnityWebRequest.Get(OnlineProjectDataUrl);
            www.timeout = 60;
            www.SendWebRequest().completed += ap =>
            {
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"<color=#088ff00>--- Load Project List Success ---</color>");
                    var tsv = www.downloadHandler.text;
                    tsv = tsv.Replace("\r\n", "\n");
                    // Debug.Log(tsv);
                    var dict = CreateProjectSettingsCfgFromTsv(tsv);
                    onLoaded?.Invoke(dict);
                }
                else
                {
                    Debug.LogError($"Loading Failed: {www.error} : {www.result} : {www.responseCode}");
                    if (EditorUtility.DisplayDialog("Download File Failed",
                            $"Loading Failed: {www.error} : {www.result} : {www.responseCode}", "OK", "Cancel"))
                    {
                        
                        if (_loadRetryTimes < NETWORK_RETRY_TIME)
                        {
                            LoadOnlineProjectSettings(onLoaded);
                            _loadRetryTimes++;
                        }
                        else
                        {
                            _loadRetryTimes = 0;
                            Debug.LogError("Load failed reach max times, check network status, plz");
                            this.Close();
                        }
                    }
                    else
                    {
                        this.Close();
                    }
                }
            };
        }


        private Dictionary<string, string> CreateProjectSettingsCfgFromTsv(string tsv)
        {
            
            var dict = new Dictionary<string, string>()
            {
                { "Default", "" },
            };

            
            var lines = tsv.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if(string.IsNullOrEmpty(line) || line.Contains("#") || !line.Contains("\t"))
                    continue; // 过滤空行

                var raw = line.Split('\t');
                if (raw.Length > 1)
                {
                    dict[raw[0]] = raw[1];
                }
                else
                {
                    dict[raw[0]] = "";
                }

            }

            return dict;
        }
        
        

        private static string GetProjectTSVUrl(string pid)
        {
            if (PublishLinks.TryGetValue(pid, out var id))
            {
                string url = string.Format(TSVLink, id);
                return url;
            }

            return "";
        }

        private void DownloadServiceTsvAndBuild(int projIndex, Action<bool, string> loadCompleted = null)
        {
            var pid = _activeNameList[projIndex];
            var uri = GetProjectTSVUrl(pid).Replace("\r", "");
            string title, msg;
            if(string.IsNullOrEmpty(uri))
            {
                title = "参数错误";
                msg = $"项目 {pid} 不正确， 请重新选择...";
                ShowDialog(title, msg);
                Debug.LogError($"{title}\n{msg}");
                return;
            }
            
            var www = UnityEngine.Networking.UnityWebRequest.Get(uri);
            www.SendWebRequest().completed += ap =>
            {
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"<color=#088ff00>--- Load Success ---</color>");
                    // Debug.Log(www.downloadHandler.text);
                    loadCompleted?.Invoke(true, www.downloadHandler.text);
                }
                else
                {
                    Debug.Log($"<color=red> Load Failed\n{uri}</color>");
                    msg = $"Result {www.result}: {www.responseCode}\n\r{www.error}";
                    Debug.LogError(msg);
                    loadCompleted?.Invoke(false, msg);
                }
            };
        }

        #endregion

        #region 文件注入项目

        private static void ResetGuruServiceInProject(string source)
        {
            // 清除所有的旧文件
            var olds = FindOldGuruServiceFile();
            if (olds.Length > 0)
            {
                foreach (var f in olds)
                {
                    File.Delete(f);
                }
            }

            // 拷贝新项目
            var fileName = new FileInfo(source).Name;
            var to = Path.GetFullPath($"{Application.dataPath}/{fileName}");
            File.Copy(source, to);
            
            
            AssetDatabase.Refresh();
            
            // 文件部署
            if (EditorUtility.DisplayDialog("\u2600 GuruServices 更新成功!", 
                    $"文件\n{fileName}\n生成成功\n\n{to}\n\n+------------------------+\n|     可为您一键导入项目     |\n+------------------------+\n", 
                    "帮我导入", 
                    "这样就好"))
            {
                GuruSDKManager.Instance.ImportAllSettings();
            }
        }


        private static string[] FindOldGuruServiceFile()
        {
            var dir = new DirectoryInfo(Application.dataPath);
            List<string> result = new List<string>();
            
            foreach (var f in dir.GetFiles("*.json", SearchOption.TopDirectoryOnly))
            {
                if (f.Name.Contains("guru-service"))
                {
                    result.Add(f.FullName);
                }
            }
            
            return result.ToArray();
        }

        #endregion
        
        #region TEST

#if GURU_SDK_DEV
        // [MenuItem("Tools/Test/Fetch Config File", false, 1)]
#endif
        private static void Test_FetchConfigFile()
        {

            var pid = "FindOut";
            var url = GetProjectTSVUrl(pid);
            
            if(string.IsNullOrEmpty(url))
            {
                Debug.LogError($"Wrong ProjectId: {pid}");
                return;
            }

            var www = UnityEngine.Networking.UnityWebRequest.Get(url);
            www.SendWebRequest().completed += ap =>
            {
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"<color=#088ff00>--- Load Success ---</color>");
                    Debug.Log(www.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"Loading Failed: {www.error} : {www.result} : {www.responseCode}");
                }
            };
            
        }

        #endregion
    }
}

#endif