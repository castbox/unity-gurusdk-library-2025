

namespace Guru.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using Guru;
    using System.Collections.Generic;
    using System.IO;
    using Facebook.Unity.Settings;
    using UnityEditor.Compilation;
#if GURU_ADJUST
    using AdjustSdk;
#endif

    public class GuruSDKManager: EditorWindow
    {
        private const string GURU_SETTINGS_PATH = "Assets/Guru/Resources/GuruSettings.asset";
        private const string APPLOVIN_SETTINGS_PATH = "Assets/Guru/Resources/AppLovinSettings.asset";
        private const string FACEBOOK_SETTINGS_PATH = "Assets/FacebookSDK/SDK/Resources/FacebookSettings.asset";
        private const string ADJUST_SETTINGS_PATH = "Assets/Guru/Editor/AdjustSettings.asset";
        private const string ANDROID_PLUGINS_DIR = "Assets/Plugins/Android";
        private const string KeyMaxAutoUpdateEnabled = "com.applovin.auto_update_enabled";
        private const string TYPE_SCRIPTABLE_OBJECT = "ScriptableObject";
        
        private const string SDK_DOCUMENT_URL = "https://docs.google.com/document/d/19S3eWpz6my6WEqZAKswsWE9fovpyI4ASzCxVSbMC5_Y";
        
        private static string ANDROID_KEYSTORE_NAME = "guru_key.jks";
        private static string GuruKeyStore => $"{ANDROID_PLUGINS_DIR}/{ANDROID_KEYSTORE_NAME}";

        
        private static GuruSDKManager _instance;
        public static GuruSDKManager Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = GetWindow<GuruSDKManager>();
                }
                return _instance;
            }
        }

        private GuruServicesConfig _serviceConfig;
        private static GUIStyle _itemTitleStyle;
        private static GUIStyle StyleItemTitle
        {
            get
            {
                if (_itemTitleStyle == null)
                {
                    _itemTitleStyle = new GUIStyle("BOX")
                    {
                        fontSize = 12,
                        fontStyle = FontStyle.Bold,
                        stretchWidth = true,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(4, 4, 4, 4),
                    };
                }
                return _itemTitleStyle;
            }
        }

        private bool _hasCheckingCompleted = false;

        private SDKMgrModel _model;


        public GuruSDKManager()
        {
            this.minSize = new Vector2(480, 680);
        }
        

        public static void Open()
        {
            Instance.Show();
        }


        private void OnEnable()
        {
            titleContent = new GUIContent("Guru SDK Manager");

            ReadServiceConfig();
            
            if (_serviceConfig != null)
            {
                Debug.Log($"<color=#88ff00>[Guru] Load <guru-services> success.</color>");
                CheckServicesCompletion();
            }
            else
            {
                Debug.Log($"<color=red>[Guru] <guru-services> file not found...</color>");
            }
            
            _model ??= SDKMgrModel.Load();

            InitPushIcon();
        }
        
        
        /// <summary>
        /// Read service from the service file again to ensure the data is latest fixed.
        /// Ensure it is not null. 
        /// </summary>
        private void ReadServiceConfig()
        {
            var config = EditorGuruServiceIO.LoadConfig();
            if (config == null)
            {
                throw new NullReferenceException("Load <guru-services> failed. Check local file plz.");
            }
            _serviceConfig = config;
        }

        #region Service Checker

        enum CheckStatus
        {
            Passed,
            Warning,
            Failed,
        }

        private const string MARK_FAIL = "#FAIL";
        private const string MARK_WARN = "#WARN";
        private const string MARK_INDENT = "    ";
        private List<string> _completionCheckResult;
        private int _serviceCriticalFail = 0;
        private int _serviceNormalFail = 0;
        
        /// <summary>
        /// 检查服务文件的配置完整性
        /// </summary>
        private void CheckServicesCompletion()
        {
            _serviceCriticalFail = 0;
            _serviceNormalFail = 0;
            
            _completionCheckResult = new List<string>(40);
            string mk_yes = " ( \u2714 ) ";
            string mk_no = " ( \u2718 ) ";
            string mk_warn = " ( ! ) ";
            string mk_star = " ( \u2605 ) ";
            string check_passed = $"{MARK_INDENT}{mk_yes} All items passed!";
            if (_serviceConfig == null)
            {
                AddResultLine($"{mk_yes} guru-services is missing", CheckStatus.Failed);
                AddResultLine($"Please contact Guru tech support to get help.", CheckStatus.Failed);
                _serviceCriticalFail++;
            }
            else
            {
                bool passed = true;
                AddResultLine($"{mk_star} <guru-services> exists!");
                if (_serviceConfig.app_settings != null 
                    && !string.IsNullOrEmpty(_serviceConfig.app_settings.bundle_id))
                {
                    AddResultLine($"{MARK_INDENT}  +  {MARK_INDENT}{_serviceConfig.app_settings.bundle_id}");
                }

                AddResultLine($"--------------------------------");
                
                //-------- APP Settings --------
                passed = true;
                AddResultLine($"[ App ]");
                if (_serviceConfig.app_settings == null)
                {
                    passed = false;
                    AddResultLine($"{MARK_INDENT}{mk_no} settings is missing!", CheckStatus.Failed);
                    _serviceCriticalFail++;
                }
                else
                {
                    if (_serviceConfig.app_settings.app_id.IsNullOrEmpty())
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_no} AppID is missing!", CheckStatus.Failed);
                        _serviceCriticalFail++;
                    }
                    if (_serviceConfig.app_settings.bundle_id.IsNullOrEmpty())
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_no} BundleID is missing!", CheckStatus.Failed);
                        _serviceCriticalFail++;
                    }
                    if (_serviceConfig.app_settings.product_name.IsNullOrEmpty())
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_no} Product Name is missing!", CheckStatus.Failed);
                        _serviceCriticalFail++;
                    }
                    if (_serviceConfig.app_settings.support_email.IsNullOrEmpty())
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_no} Support Email is missing!", CheckStatus.Failed);
                        _serviceNormalFail++;
                    }
                    if (_serviceConfig.app_settings.custom_keystore)
                    {
                        AddResultLine($"{MARK_INDENT}{mk_warn} Using Custom Keystore.", CheckStatus.Warning);
                    }
                }
                
                if (passed) AddResultLine(check_passed);
                
                //-------- ADS Settings --------
                passed = true;
                AddResultLine($"[ Ads ]");
                if (_serviceConfig.ad_settings == null)
                {
                    passed = false;
                    AddResultLine($"{MARK_INDENT}{mk_no} settings is missing!", CheckStatus.Failed);
                    _serviceCriticalFail++;
                }
                else
                {
                    if (_serviceConfig.ad_settings.sdk_key.IsNullOrEmpty())
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_no} SDK Key is missing!", CheckStatus.Failed);
                        _serviceCriticalFail++;
                    }
                    if (!IsArrayNotEmpty(_serviceConfig.ad_settings.admob_app_id))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_no} Admob ID is missing!", CheckStatus.Failed);
                        _serviceCriticalFail++;
                    }
                    if (!IsArrayNotEmpty(_serviceConfig.ad_settings.max_ids_android))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_no} AppLovin Android IDs is missing!", CheckStatus.Failed);
                        _serviceCriticalFail++;
                    }
                    if (!IsArrayNotEmpty(_serviceConfig.ad_settings.max_ids_ios))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_no} AppLovin iOS IDs is missing!", CheckStatus.Failed);
                        _serviceCriticalFail++;
                    }
                    if (!IsArrayNotEmpty(_serviceConfig.ad_settings.amazon_ids_android))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Amazon Android IDs is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }
                    if (!IsArrayNotEmpty(_serviceConfig.ad_settings.amazon_ids_ios))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Amazon iOS IDs is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }
                    if (!IsArrayNotEmpty(_serviceConfig.ad_settings.pubmatic_ids_android))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Pubmatic Android IDs is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }
                    if (!IsArrayNotEmpty(_serviceConfig.ad_settings.pubmatic_ids_ios))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Pubmatic iOS IDs is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }
                    if (!IsArrayNotEmpty(_serviceConfig.ad_settings.moloco_ids_android))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Moloco Android Test IDs is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }
                    if (!IsArrayNotEmpty(_serviceConfig.ad_settings.moloco_ids_ios))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Moloco iOS Test IDs is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }
                    if (!IsArrayNotEmpty(_serviceConfig.ad_settings.tradplus_ids_android))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Tradplus Android Test IDs is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }
                    if (!IsArrayNotEmpty(_serviceConfig.ad_settings.tradplus_ids_ios))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Tradplus iOS Test IDs is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }
                }
                if (passed) AddResultLine(check_passed);
                
                //-------- Channels Settings --------
                passed = true;
                AddResultLine($"[ Channels ]");
                if (_serviceConfig.fb_settings == null)
                {
                    passed = false;
                    AddResultLine($"{MARK_INDENT}{mk_warn} Facebook settings is missing!", CheckStatus.Warning);
                    _serviceNormalFail++;
                }
                else
                {
                    if (_serviceConfig.fb_settings.fb_app_id.IsNullOrEmpty())
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Facebook AppID is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }
                    if (_serviceConfig.fb_settings.fb_client_token.IsNullOrEmpty())
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Facebook Client Token is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }
                }

                if (_serviceConfig.adjust_settings == null)
                {
                    passed = false;
                    AddResultLine($"{MARK_INDENT}{mk_warn} Adjust settings is missing!", CheckStatus.Warning);
                    _serviceNormalFail++;
                }
                else
                {
                    if(!IsArrayNotEmpty(_serviceConfig.adjust_settings.app_token))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Adjust AppToken is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }

                    if (!IsArrayNotEmpty(_serviceConfig.adjust_settings.events))
                    {
                        passed = false;
                        AddResultLine($"{MARK_INDENT}{mk_warn} Adjust Events is missing!", CheckStatus.Warning);
                        _serviceNormalFail++;
                    }
                }
                if (passed) AddResultLine(check_passed);
                
                //-------- IAP --------
                passed = true;
                AddResultLine($"[ IAP ]");
                if (!IsArrayNotEmpty(_serviceConfig.products))
                {
                    passed = false;
                    AddResultLine($"{MARK_INDENT}{mk_warn} Product list is missing!", CheckStatus.Warning);
                    _serviceNormalFail++;
                }
                if (passed) AddResultLine(check_passed);
            }

        }

        private void AddResultLine(string msg, CheckStatus status = CheckStatus.Passed)
        {
            if (_completionCheckResult == null)
            {
                _completionCheckResult = new List<string>(40);
            }

            string mk = "";
            switch (status)
            {
                case CheckStatus.Passed:
                    mk = ""; 
                    break;
                case CheckStatus.Warning:
                    mk = MARK_WARN;
                    break;
                case CheckStatus.Failed:
                    mk = MARK_FAIL;
                    break;
            }
            _completionCheckResult.Add($"{msg}{mk}");
        }

        private void GUI_ServiceCheckResult()
        {
            if (_completionCheckResult != null)
            {
                Color green = new Color(0.7f, 1, 0);
                Color red = new Color(1, 0.2f, 0);
                Color yellow = new Color(1f, 0.8f, 0.2f);
                Color c;
                string line = "";
                for (int i = 0; i < _completionCheckResult.Count; i++)
                {
                    c = green;
                    line = _completionCheckResult[i];
                    if (line.EndsWith(MARK_FAIL))
                    {
                        line = line.Replace(MARK_FAIL, "");
                        c = red;
                    }
                    else if (line.EndsWith(MARK_WARN))
                    {
                        line = line.Replace(MARK_WARN, "");
                        c = yellow;
                    }
                    GUI_Color(c, () =>
                    {
                        EditorGUILayout.LabelField(line);
                        GUILayout.Space(2);
                    });
                    
                }
            }

        }


        #endregion
        
        #region GUI

        void OnGUI()
        {
            // TITLE
            GUI_WindowTitle();
            
            // CONTENT
            if (_serviceConfig == null)
            {
                GUI_OnConfigDisabled();
            }
            else
            {
                GUI_OnConfigEnabled();
            }
            
            // Push
            GUILayout.Space(10);
            GUI_PushIconMaker();
            
            // Doc
            GUILayout.Space(10);
            GUI_JumpToDocument();

            // Keystore
            // if (_serviceConfig != null && _serviceConfig.UseCustomKeystore())
            // {
            //     GUILayout.Space(10);
            //     GUI_CustomKeystore();
            // }

        }

        private void GUI_WindowTitle()
        {
            GUILayout.Space(4);
            
            var s = new GUIStyle("BOX")
            {
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                stretchHeight = true,
                fixedHeight = 60,
            };


            EditorGUILayout.LabelField("Guru SDK",s);
            s.fontSize = 13;
            s.fixedHeight = 20;
            EditorGUILayout.LabelField($"Version {GuruSDK.Version}", s);
            
            GUILayout.Space(4);
        }


        /// <summary>
        /// 配置不可用
        /// </summary>
        private void GUI_OnConfigDisabled()
        {
            GUI_Color(new Color(1,0.2f, 0), () =>
            {
                EditorGUILayout.LabelField("<guru-services> file not found! \nPlease contact Guru tech support to solve the problem. ", StyleItemTitle);
            });
        }


        /// <summary>
        /// 配置可用
        /// </summary>
        private void GUI_OnConfigEnabled()
        {
            var box = new GUIStyle("BOX");
            float btnH = 40;
            
            //------------ check allsettings -------------
            EditorGUILayout.LabelField("[ Guru Service ]", StyleItemTitle);
            GUILayout.Space(2);
            GUI_ServiceCheckResult();
            GUILayout.Space(16);

            if (_serviceCriticalFail > 0)
            {
                // 严重错误过多
            }
            else
            {
                GUI_Button("IMPORT ALL SETTINGS", () =>
                {
                    ImportAllSettings();
                }, null, GUILayout.Height(btnH));
            }
            
            GUILayout.Space(4);
            
        }

        /// <summary>
        /// 执行其他的命令
        /// </summary>
        private void ExecuteAdditionalCommands()
        {
            // 执行其他的命令
            RemoveOldAdjustSignatureFiles();
            Add16KbFilesForAndroid();
        }

        
        /// <summary>
        /// 导入所有配置
        /// </summary>
        public void ImportAllSettings()
        {
            ReadServiceConfig(); // Read file again
            CheckAllComponents();
            ExecuteAdditionalCommands();
        }


        #endregion
        
        #region Check Components

        private string logBuffer;
        
        
        private void CheckAllComponents()
        {
            float progress = 0;
            string barTitle = "Setup All Components";
            EditorUtility.DisplayCancelableProgressBar(barTitle, "Start collect all components", progress);
            Debug.Log("--- Setup All Components ---");
            ApplyMods();
            progress += 0.1f;
            EditorUtility.DisplayCancelableProgressBar(barTitle, "Setup Mods is done", progress);  
            ImportGuruSettings();
            progress += 0.2f;
            EditorUtility.DisplayCancelableProgressBar(barTitle, "GuruSettings is done", progress);
            ImportAppLovinSettings();
            progress += 0.2f;
            EditorUtility.DisplayCancelableProgressBar(barTitle, "AppLovinSettings is done", progress);
            ImportFacebookSettings();
            progress += 0.2f;
            EditorUtility.DisplayCancelableProgressBar(barTitle, "FacebookSettings is done", progress);
            ImportAdjustSettings();
            progress += 0.2f;
            EditorUtility.DisplayCancelableProgressBar(barTitle, "AdjustSettings is done", progress);
            CheckAndroidKeyStore();
            EditorGuruServiceIO.DeployLocalServiceFile(); // 部署文件
            progress += 0.1f;
            EditorUtility.DisplayCancelableProgressBar(barTitle, "Deploy service file...", progress);
            
            AssetDatabase.SaveAssets();
            
            CompilationPipeline.RequestScriptCompilation();
            CompilationPipeline.compilationFinished += _ =>
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Importing Guru Services", "All the settings importing success!", "OK");
            };
        }

        //------------------------- GuruSettings --------------------------------
        private void ImportGuruSettings()
        {
            GuruSettings settings = null;
            if (IsAssetExists(nameof(GuruSettings), TYPE_SCRIPTABLE_OBJECT, GURU_SETTINGS_PATH, true))
            {
                settings = AssetDatabase.LoadAssetAtPath<GuruSettings>(GURU_SETTINGS_PATH);
            }
            else
            {
                EnsureParentDirectory(GURU_SETTINGS_PATH);
                settings = CreateInstance<GuruSettings>();
                AssetDatabase.CreateAsset(settings, GURU_SETTINGS_PATH);
            }
            settings.CompanyName = "Guru";
            settings.ProductName = _serviceConfig.app_settings.product_name;
            settings.GameIdentifier = _serviceConfig.app_settings.bundle_id;
            settings.PriacyUrl = _serviceConfig.app_settings.privacy_url;
            settings.TermsUrl = _serviceConfig.app_settings.terms_url;
            settings.SupportEmail = _serviceConfig.app_settings.support_email;
            settings.AndroidStoreUrl = _serviceConfig.app_settings.android_store;
            settings.IOSStoreUrl = _serviceConfig.app_settings.ios_store;
            
            SerializedObject so = new SerializedObject(settings);
            SerializedProperty n;
            SerializedObject nn;
            SerializedProperty p;
            string[] arr;
            
            n = so.FindProperty("IPMSetting");
            if (null != n)
            {
                // AppID
                p = n.serializedObject.FindProperty("IPMSetting.appID");
                if (p != null) p.stringValue = _serviceConfig.app_settings.app_id;
                // BundleID
                p = n.serializedObject.FindProperty("IPMSetting.bundleId");
                if (p != null) p.stringValue = _serviceConfig.app_settings.bundle_id;
                // CdnHost
                p = n.serializedObject.FindProperty("IPMSetting.cdnHost");
                if (p != null) p.stringValue = _serviceConfig.GetCdnHost();
                // CdnHost
                p = n.serializedObject.FindProperty("IPMSetting.usingUUID");
                if (p != null) p.boolValue = _serviceConfig.GetUsingUUID();
                // tokenValidTime
                if (_serviceConfig.GetTokenValidTime() > 0)
                {
                    p = n.serializedObject.FindProperty("IPMSetting.tokenValidTime");
                    if (p != null) p.intValue = _serviceConfig.GetTokenValidTime();
                }
                if (_serviceConfig.fb_settings != null)
                {
                    // FB App ID
                    p = n.serializedObject.FindProperty("IPMSetting.fbAppId");
                    if (p != null) p.stringValue = _serviceConfig.fb_settings.fb_app_id;
                    // FB Client Token
                    p = n.serializedObject.FindProperty("IPMSetting.fbClientToken");
                    if (p != null) p.stringValue = _serviceConfig.fb_settings.fb_client_token;
                }
            }
            
            //---------- AMAZON -----------------------
            n = so.FindProperty("AmazonSetting");
            if (null != n)
            {
                p = n.serializedObject.FindProperty("AmazonSetting.Enable");
                if (p != null) p.boolValue = true;
                
                arr = _serviceConfig.ad_settings.amazon_ids_android;
                if (IsArrayHasLength(arr, 4))
                {
                    p = n.serializedObject.FindProperty("AmazonSetting.Android.appID");
                    if (p != null) p.stringValue = arr[0];
                    p = n.serializedObject.FindProperty("AmazonSetting.Android.bannerSlotID");
                    if (p != null) p.stringValue = arr[1];
                    p = n.serializedObject.FindProperty("AmazonSetting.Android.interSlotID");
                    if (p != null) p.stringValue = arr[2];
                    p = n.serializedObject.FindProperty("AmazonSetting.Android.rewardSlotID");
                    if (p != null) p.stringValue = arr[3];
                }

                arr = _serviceConfig.ad_settings.amazon_ids_ios;
                if (IsArrayHasLength(arr, 4))
                {
                    p = n.serializedObject.FindProperty("AmazonSetting.iOS.appID");
                    if (p != null) p.stringValue = arr[0];
                    p = n.serializedObject.FindProperty("AmazonSetting.iOS.bannerSlotID");
                    if (p != null) p.stringValue = arr[1];
                    p = n.serializedObject.FindProperty("AmazonSetting.iOS.interSlotID");
                    if (p != null) p.stringValue = arr[2];
                    p = n.serializedObject.FindProperty("AmazonSetting.iOS.rewardSlotID");
                    if (p != null) p.stringValue = arr[3];
                }
            }
            
            //---------- PUBMATIC -----------------------
            n = so.FindProperty("PubmaticSetting");
            if (null != n)
            {
                p = n.serializedObject.FindProperty("PubmaticSetting.Enable");
                if (p != null) p.boolValue = true;

                p = n.serializedObject.FindProperty("PubmaticSetting.Android.storeUrl");
                if (p != null) p.stringValue = _serviceConfig.app_settings.android_store;

                p = n.serializedObject.FindProperty("PubmaticSetting.iOS.storeUrl");
                if (p != null) p.stringValue = _serviceConfig.app_settings.ios_store;
                
                arr = _serviceConfig.ad_settings.pubmatic_ids_android;
                if (IsArrayHasLength(arr, 3))
                {
                    p = n.serializedObject.FindProperty("PubmaticSetting.Android.bannerUnitID");
                    if (p != null) p.stringValue = arr[0];
                    p = n.serializedObject.FindProperty("PubmaticSetting.Android.interUnitID");
                    if (p != null) p.stringValue = arr[1];
                    p = n.serializedObject.FindProperty("PubmaticSetting.Android.rewardUnitID");
                    if (p != null) p.stringValue = arr[2];
                }

                arr = _serviceConfig.ad_settings.pubmatic_ids_ios;
                if (IsArrayHasLength(arr, 3))
                {
                    p = n.serializedObject.FindProperty("PubmaticSetting.iOS.bannerUnitID");
                    if (p != null) p.stringValue = arr[0];
                    p = n.serializedObject.FindProperty("PubmaticSetting.iOS.interUnitID");
                    if (p != null) p.stringValue = arr[1];
                    p = n.serializedObject.FindProperty("PubmaticSetting.iOS.rewardUnitID");
                    if (p != null) p.stringValue = arr[2];
                }
            }

            //---------- MOLOCO -----------------------
            n = so.FindProperty("MolocoSetting");
            if (null != n)
            {
                p = n.serializedObject.FindProperty("MolocoSetting.Enable");
                if (p != null) p.boolValue = true;
                
                arr = _serviceConfig.ad_settings.moloco_ids_android;
                if (IsArrayHasLength(arr, 3))
                {
                    p = n.serializedObject.FindProperty("MolocoSetting.Android.bannerTestUnitID");
                    if (p != null) p.stringValue = arr[0];
                    p = n.serializedObject.FindProperty("MolocoSetting.Android.interTestUnitID");
                    if (p != null) p.stringValue = arr[1];
                    p = n.serializedObject.FindProperty("MolocoSetting.Android.rewardTestUnitID");
                    if (p != null) p.stringValue = arr[2];
                }

                arr = _serviceConfig.ad_settings.moloco_ids_ios;
                if (IsArrayHasLength(arr, 3))
                {
                    p = n.serializedObject.FindProperty("MolocoSetting.iOS.bannerTestUnitID");
                    if (p != null) p.stringValue = arr[0];
                    p = n.serializedObject.FindProperty("MolocoSetting.iOS.interTestUnitID");
                    if (p != null) p.stringValue = arr[1];
                    p = n.serializedObject.FindProperty("MolocoSetting.iOS.rewardTestUnitID");
                    if (p != null) p.stringValue = arr[2];
                }
            }
            
            //---------- TRADPLUS -----------------------
            n = so.FindProperty("TradplusSetting");
            if (null != n)
            {
                arr = _serviceConfig.ad_settings.tradplus_ids_android;
                if (IsArrayHasLength(arr, 3))
                {
                    p = n.serializedObject.FindProperty("TradplusSetting.Android.bannerUnitID");
                    if (p != null) p.stringValue = arr[0];
                    p = n.serializedObject.FindProperty("TradplusSetting.Android.interUnitID");
                    if (p != null) p.stringValue = arr[1];
                    p = n.serializedObject.FindProperty("TradplusSetting.Android.rewardUnitID");
                    if (p != null) p.stringValue = arr[2];
                }
                
                arr = _serviceConfig.ad_settings.tradplus_ids_ios;
                if (IsArrayHasLength(arr, 3))
                {
                    p = n.serializedObject.FindProperty("TradplusSetting.iOS.bannerUnitID");
                    if (p != null) p.stringValue = arr[0];
                    p = n.serializedObject.FindProperty("TradplusSetting.iOS.interUnitID");
                    if (p != null) p.stringValue = arr[1];
                    p = n.serializedObject.FindProperty("TradplusSetting.iOS.rewardUnitID");
                    if (p != null) p.stringValue = arr[2];
                }
            }

            //----------- ADSettings -------------------
            n = so.FindProperty("ADSetting");
            if (null != n)
            {
                p = n.serializedObject.FindProperty("ADSetting.SDK_KEY");
                if (p != null) p.stringValue = _serviceConfig.ad_settings.sdk_key;
                
                arr = _serviceConfig.ad_settings.max_ids_android;
                if(IsArrayHasLength(arr, 3))
                {
                    p = n.serializedObject.FindProperty("ADSetting.Android_Banner_ID");
                    if (p != null) p.stringValue = arr[0];
                    p = n.serializedObject.FindProperty("ADSetting.Android_Interstitial_ID");
                    if (p != null) p.stringValue = arr[1];
                    p = n.serializedObject.FindProperty("ADSetting.Android_Rewarded_ID");
                    if (p != null) p.stringValue = arr[2];
                }
                
                arr = _serviceConfig.ad_settings.max_ids_ios;
                if (IsArrayHasLength(arr, 3))
                {
                    p = n.serializedObject.FindProperty("ADSetting.IOS_Banner_ID");
                    if (p != null) p.stringValue = arr[0];
                    p = n.serializedObject.FindProperty("ADSetting.IOS_Interstitial_ID");
                    if (p != null) p.stringValue = arr[1];
                    p = n.serializedObject.FindProperty("ADSetting.IOS_Rewarded_ID");
                    if (p != null) p.stringValue = arr[2];
                }
            }
            
            //----------- AdjustSetting -------------------
            n = so.FindProperty("AdjustSetting");
            if (null != n 
                && IsArrayHasLength(_serviceConfig.adjust_settings.app_token, 2))
            {
                p = n.serializedObject.FindProperty("AdjustSetting.androidAppToken");
                if (p != null) p.stringValue = _serviceConfig.adjust_settings.app_token[0];
                
                p = n.serializedObject.FindProperty("AdjustSetting.iOSAppToken");
                if (p != null) p.stringValue = _serviceConfig.adjust_settings.app_token[1];
            }

            //----------- AnalyticsSetting -------------------
            n = so.FindProperty("AnalyticsSetting");
            if (null != n)
            {
                p = n.serializedObject.FindProperty("AnalyticsSetting.levelEndSuccessNum");
                if (p != null) p.intValue = _serviceConfig.GetLevelEndSuccessNum();
                p = n.serializedObject.FindProperty("AnalyticsSetting.enalbeFirebaseAnalytics");
                if (p != null) p.boolValue = _serviceConfig.IsFirebaseEnabled();
                p = n.serializedObject.FindProperty("AnalyticsSetting.enalbeFacebookAnalytics");
                if (p != null) p.boolValue = _serviceConfig.IsFacebookEnabled();
                p = n.serializedObject.FindProperty("AnalyticsSetting.enalbeAdjustAnalytics");
                if (p != null) p.boolValue = _serviceConfig.IsAdjustEnabled();
                p = n.serializedObject.FindProperty("AnalyticsSetting.adjustEventList");
                if (null != p && IsArrayNotEmpty(_serviceConfig.adjust_settings.events))
                {
                    p.ClearArray();
                    for (int i = 0; i < _serviceConfig.adjust_settings.events.Length; i++)
                    {
                        arr = _serviceConfig.adjust_settings.events[i].Split(',');
                        if (IsArrayHasLength(arr, 3))
                        {
                            p.InsertArrayElementAtIndex(i);
                            nn = p.GetArrayElementAtIndex(i).serializedObject;
                            nn.FindProperty($"AnalyticsSetting.adjustEventList.Array.data[{i}].EventName").stringValue = arr[0];
                            nn.FindProperty($"AnalyticsSetting.adjustEventList.Array.data[{i}].AndroidToken").stringValue = arr[1];
                            nn.FindProperty($"AnalyticsSetting.adjustEventList.Array.data[{i}].IOSToken").stringValue = arr[2];
                        }
                    }
                }
            }
            
            //---------------- Productions ------------------------
            n = so.FindProperty("Products");
            if (n != null && IsArrayNotEmpty(_serviceConfig.products))
            {
                n.ClearArray();
                for (int i = 0; i < _serviceConfig.products.Length; i++)
                {
                    arr = _serviceConfig.products[i].Split(',');
                    if (IsArrayHasLength(arr, 5))
                    {
                        n.InsertArrayElementAtIndex(i);
                        nn = n.GetArrayElementAtIndex(i).serializedObject;
                        nn.FindProperty($"Products.Array.data[{i}].ProductName").stringValue = arr[0];
                        nn.FindProperty($"Products.Array.data[{i}].GooglePlayProductId").stringValue = arr[1];
                        nn.FindProperty($"Products.Array.data[{i}].AppStoreProductId").stringValue = arr[2];
                        nn.FindProperty($"Products.Array.data[{i}].Price").doubleValue = double.Parse(arr[3]);
                        nn.FindProperty($"Products.Array.data[{i}].Type").enumValueIndex = int.Parse(arr[4]);
                        nn.FindProperty($"Products.Array.data[{i}].Category").stringValue = "Store";
                        nn.FindProperty($"Products.Array.data[{i}].IsFree").boolValue = false;
                        
                        //----- Extends Columes ------
                        if (arr.Length > 5) 
                            nn.FindProperty($"Products.Array.data[{i}].Category").stringValue = arr[5];
                        if (arr.Length > 6)
                            nn.FindProperty($"Products.Array.data[{i}].IsFree").boolValue = arr[6].ToLower() == "1" || arr[6].ToLower() == "true";
                    } 
                }
            }

            //------- Save SO ----------
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
        }

        //------------------------- AppLovinSettings --------------------------------
        private void ImportAppLovinSettings()
        {
            EditorPrefs.SetBool(KeyMaxAutoUpdateEnabled, false); // 关闭Max自动升级功能
            
            AppLovinSettings settings = null;
            if (IsAssetExists(nameof(AppLovinSettings), TYPE_SCRIPTABLE_OBJECT, APPLOVIN_SETTINGS_PATH, true))
            {
                settings = AssetDatabase.LoadAssetAtPath<AppLovinSettings>(APPLOVIN_SETTINGS_PATH);
            }
            else
            {
                settings = CreateInstance<AppLovinSettings>();
                AssetDatabase.CreateAsset(settings, APPLOVIN_SETTINGS_PATH);
            }

            settings.SetAttributionReportEndpoint = true;
            settings.QualityServiceEnabled = true;
            settings.AddApsSkAdNetworkIds = true;
            settings.SdkKey = _serviceConfig.ad_settings.sdk_key;
            if (IsArrayHasLength(_serviceConfig.ad_settings.admob_app_id, 2))
            {
                settings.AdMobAndroidAppId = _serviceConfig.ad_settings.admob_app_id[0];
                settings.AdMobIosAppId = _serviceConfig.ad_settings.admob_app_id[1];
            }
            // settings.ConsentFlowEnabled = false;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
        }

        //------------------------- FacebookSettings --------------------------------
        private void ImportFacebookSettings()
        {
            FacebookSettings settings = null;
            string p = FindAssetPath(nameof(FacebookSettings), "ScriptableObject");
            if (!string.IsNullOrEmpty(p))
            {
                settings = AssetDatabase.LoadAssetAtPath<FacebookSettings>(p);
            }
            else
            {
                EnsureParentDirectory(FACEBOOK_SETTINGS_PATH);
                settings = CreateInstance<FacebookSettings>();
                AssetDatabase.CreateAsset(settings, FACEBOOK_SETTINGS_PATH);
            }

            var so = new SerializedObject(settings);
            SerializedProperty n;
            
            n = so.FindProperty("appLabels");
            if (n != null)
            {
                n.ClearArray();
                n.InsertArrayElementAtIndex(0);
                n.GetArrayElementAtIndex(0).stringValue = _serviceConfig.app_settings.product_name;
            }

            n = so.FindProperty("appIds");
            if (n != null)
            {
                n.ClearArray();
                n.InsertArrayElementAtIndex(0);
                n.GetArrayElementAtIndex(0).stringValue = _serviceConfig.fb_settings.fb_app_id;
            }
            
            n = so.FindProperty("clientTokens");
            if (n != null)
            {
                n.ClearArray();
                n.InsertArrayElementAtIndex(0);
                n.GetArrayElementAtIndex(0).stringValue = _serviceConfig.fb_settings.fb_client_token;
            }


            string ks_path = GuruKeyStore;
            if (_serviceConfig?.UseCustomKeystore() ?? false)
            {
                ks_path = _model?.KeyStorePath ?? "";
            }
            n = so.FindProperty("androidKeystorePath");
            if (n != null && File.Exists(ks_path))
            {
                n.stringValue = ks_path;
            }
            
            Facebook.Unity.Editor.ManifestMod.GenerateManifest(); // 重新生成 Manifest
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
        }
        
        //------------------------- AdjustSettings --------------------------------
        private void ImportAdjustSettings()
        {
#if GURU_ADJUST
            AdjustSettings settings = null;
            string p = FindAssetPath(nameof(AdjustSettings), "ScriptableObject");
            if (!string.IsNullOrEmpty(p))
            {
                settings = AssetDatabase.LoadAssetAtPath<AdjustSettings>(p);
            }
            else
            {
                var dir = Directory.GetParent(ADJUST_SETTINGS_PATH);
                if(dir != null && !dir.Exists) dir.Create();
                
                settings = CreateInstance<AdjustSettings>();
                AssetDatabase.CreateAsset(settings, ADJUST_SETTINGS_PATH);
            }

            var urlSchemaList = _serviceConfig.GetUrlSchemaList();
            if (urlSchemaList == null || urlSchemaList.Length == 0)
            {
                Debug.LogWarning($"No url-schema is found. skip import.");
                return;
            }
            
            var so = new SerializedObject(settings);
            SerializedProperty n;

            // 注入 Android Schema: https://dev.adjust.com/en/sdk/android/features/deep-links/?version=v4
            // 注意 Android Schema 直接设置 AdjustSettings 实测打包后无法生效。
            // 需要在注入GuruSDK 数据的时候单独注入到 AndroidManifest 内才能生效
            if (urlSchemaList.Length > 0)
            {
                var androidUrlSchemes = urlSchemaList[0].Split(',');
                if (androidUrlSchemes != null && androidUrlSchemes.Length > 0)
                {
                    n = so.FindProperty("androidUriSchemes");
                    if (n != null)
                    {
                        n.ClearArray();
                        for (int i = 0; i < androidUrlSchemes.Length; i++)
                        {
                            n.InsertArrayElementAtIndex(i);
                            n.GetArrayElementAtIndex(i).stringValue = androidUrlSchemes[i];
                        }
                    }
                }
            }
            
            // 注入 iOS Schema: https://dev.adjust.com/zh/sdk/unity/features/deep-links?version=v4
            // 注意 iOS Schema 直接设置 AdjustSettings 是可以生效的。
            if (urlSchemaList.Length > 1)
            {
                var iosUrlSchemes = urlSchemaList[1].Split(',');
                if (iosUrlSchemes != null && iosUrlSchemes.Length > 0)
                {
                    n = so.FindProperty("_iOSUniversalLinksDomains");
                    if (n != null)
                    {
                        n.ClearArray();
                        for (int i = 0; i < iosUrlSchemes.Length; i++)
                        {
                            var iosSchema = iosUrlSchemes[i].Replace("https://", "applinks:");
                            n.InsertArrayElementAtIndex(i);
                            n.GetArrayElementAtIndex(i).stringValue = iosSchema;
                        }
                    }
                    
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
#endif
        }
        
        
        

        //------------------------- Android Keystore ----------------------------------

        private void CheckAndroidKeyStore()
        {
            if (!_serviceConfig.UseCustomKeystore())
            {
                DeployGuruKeystore();
            }
        }

        private void DeployGuruKeystore()
        {
            var from = "Packages/com.guru.unity.sdk.core/Editor/BuildTool/guru_key.jks";
            var to = GuruKeyStore.Replace("Assets", Application.dataPath);
            if (File.Exists(from) && !File.Exists(to))
            {
                File.Copy(from, to);
            }
        }


        private void ApplyMods()
        {
            PlayerSettings.applicationIdentifier = _serviceConfig.app_settings.bundle_id; // 设置包名
            
            //-------- 部署 Android 相关的文件和资源 ----------
            AndroidManifestMod.Apply(GetAndroidUrlSchemaList());
            AndroidProjectMod.Apply();
            AndroidResMod.Apply(); // 部署 GuruSDKRes 资源文件, 目前部署在 guru.sdk.core 的 Plugins/Android/SDKRes.androidlib 中
            
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 获取 Android 端的 UrlSchema 配置
        /// </summary>
        /// <returns></returns>
        private string GetAndroidUrlSchemaList()
        {
#if UNITY_ANDROID
            var list = _serviceConfig.GetUrlSchemaList();
            if (list != null && list.Length > 0) return list[0];
#endif
            return "";
        }


        #endregion

        #region GUI Utils
        

        private void GUI_Color(Color color, Action content)
        {
            var c = GUI.color;
            GUI.color = color;
            content?.Invoke();
            GUI.color = c;
        }
        
        private void GUI_Button(string label, Action content, Color color, GUIStyle style = null, params GUILayoutOption[] options)
        {
            GUI_Color(color, ()=> GUI_Button(label, content, style, options));
        }
        
        private void GUI_Button(string label, Action content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style != null)
            {
                if (GUILayout.Button(label,style, options))
                {
                    content?.Invoke();
                }
            }
            else
            {
                if (GUILayout.Button(label, options))
                {
                    content?.Invoke();
                }
            }
        }


        #endregion

        #region Utils
        
        /// <summary>
        /// 插着组件路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static string FindAssetPath(string assetName, string typeName = "")
        {
            string filter = assetName;
            if (!string.IsNullOrEmpty(typeName)) filter = $"{assetName} t:{typeName}";
            var guids = AssetDatabase.FindAssets(filter);

            string p = "";
            if (guids != null && guids.Length > 0)
            {
                for (int i = 0; i < guids.Length; i++)
                {
                     p = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (File.Exists(p.Replace("Assets", Application.dataPath)))
                    {
                        return p;
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// 获取Assets路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="typeName"></param>
        /// <param name="defaultPath"></param>
        /// <param name="deleteOthers"></param>
        /// <returns></returns>
        private static bool IsAssetExists(string assetName,string typeName, string defaultPath, bool deleteOthers = false)
        {
            bool result = false;
            var guids = AssetDatabase.FindAssets($"{assetName} t:{typeName}");
            string p = "";
            if (guids != null && guids.Length > 0)
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    p = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (File.Exists(p))
                    {
                        if (p == defaultPath)
                        {
                            result = true;
                        }
                        else
                        {
                            if(deleteOthers) File.Delete(p);
                        }
                    }
                }
            }
            return result;
        }

        private static bool IsArrayNotEmpty(Array array)
        {
            if (array == null) return false;
            if (array.Length == 0) return false;
            return true;
        }

        private static bool IsArrayHasLength(Array array, int length)
        {
            if(!IsArrayNotEmpty(array)) return false;
            return array.Length >= length;
        }

        private static void EnsureParentDirectory(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(dir)) return;
            
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        #endregion
        
        #region Push Icon Maker

        private bool _showSegmentPush = false;
        private Texture2D _pushIconSource;
        private Color _pushIconColor = Color.white;
        
        private void InitPushIcon()
        {
            if (_pushIconSource == null)
            {
                var path = _model?.PushIconPath ?? "";
                if(!string.IsNullOrEmpty(path))
                {
                    _pushIconSource = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                
            }
        }
        
        private void GUI_PushIconMaker()
        {
            float btnH = 24;
            
            // EditorGUILayout.LabelField("[ Push Icon ]", StyleItemTitle);
            _showSegmentPush = EditorGUILayout.Foldout(_showSegmentPush, "[ Android Push Icon ]");
            if (_showSegmentPush)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField("Icon ( 96x96 PNG )");
                _pushIconSource = EditorGUILayout.ObjectField( _pushIconSource, typeof(Texture2D), false) as Texture2D;
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField("Icon Color");
                _pushIconColor = EditorGUILayout.ColorField(_pushIconColor);
                EditorGUILayout.EndHorizontal();
            
                if (null != _pushIconSource)
                {
                    GUI_Button("CREATE PUSH ASSETS", () =>
                    {
                        if (AndroidPushIconHelper.SetPushIconAssets(_pushIconSource, _pushIconColor))
                        {
                            EditorUtility.DisplayDialog("Set Push Icon", "Push Icon assets created success!", "OK");
                            var path = AssetDatabase.GetAssetPath(_pushIconSource);
                            if(_model != null) _model.PushIconPath = path;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Set Push Icon", "Push Icon assets created failed!", "Well...");
                        }

                        _showSegmentPush = false;
                    }, null, GUILayout.Height(btnH));
                }
                
                EditorGUI.indentLevel--;
            }
        }

        
        private bool _showSegmentDoc = false;
        private void GUI_JumpToDocument()
        {
            _showSegmentDoc = EditorGUILayout.Foldout(_showSegmentDoc, "[ Documents ]");
            if (_showSegmentDoc)
            {
                EditorGUI.indentLevel++;
                var s = new GUIStyle("label")
                {
                    alignment = TextAnchor.MiddleLeft,
                    normal =
                    {
                        textColor = Color.cyan
                    }
                };
                EditorGUILayout.BeginHorizontal("box");
                GUI_Button("    \u2192 打开 GuruSDK 接入文档", () => { Application.OpenURL(SDK_DOCUMENT_URL); }, s);
                EditorGUILayout.EndHorizontal();
             
                EditorGUI.indentLevel--;
            }
        }


        #endregion

        #region Custom Keystore

        private bool _showSegmentCustomKeystore = false;
        private string _androidKeystorePath = "";
        
        private void InitCustomKeystore()
        {
            if (_model != null)
            {
                if (!string.IsNullOrEmpty(_model.KeyStorePath))
                {
                    _androidKeystorePath = _model.KeyStorePath;
                }
            }
        }

        private void GUI_CustomKeystore()
        {
            float btnH = 24;
            _showSegmentCustomKeystore = EditorGUILayout.Foldout(_showSegmentCustomKeystore, "[ Android Keystore ]");
            if (_showSegmentCustomKeystore)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField("Keystore Path");
                _androidKeystorePath = EditorGUILayout.TextField(_androidKeystorePath);
                if (GUILayout.Button("...", GUILayout.Width(24)))
                {
                    var path = EditorUtility.OpenFilePanel("Select Keystore File", Path.GetFullPath($"{Application.dataPath}/Plugins/Android"), "jks");
                    if(File.Exists(path) && path != _androidKeystorePath)
                    {
                        _androidKeystorePath = path;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                if (!string.IsNullOrEmpty(_androidKeystorePath))
                {
                    GUI_Button("SET KEYSTORE", () =>
                    {
                        if (ApplyCustomKeystore(_androidKeystorePath))
                        {
                            EditorUtility.DisplayDialog("Set Keystore", "Keystore path set success!", "OK");
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Set Keystore", "Keystore file not found!", "Well...");
                        }
                    }, null, GUILayout.Height(btnH));
                }
                
                EditorGUI.indentLevel--;
            }
        }

        private bool ApplyCustomKeystore(string storePath)
        {
            if (!string.IsNullOrEmpty(storePath))
            {
                var file = new FileInfo(storePath);
                if (file.Exists)
                {
                    var from = storePath;
                    var setPath = $"{ANDROID_PLUGINS_DIR}/{file.Name}";
                    var to = setPath.Replace("Assets", Application.dataPath);
                    Debug.Log($"---- fileName: {file.Name}"  );
                    if (!File.Exists(to))
                    {
                        File.Copy(from, to);
                    }

                    if (File.Exists(to))
                    {
                        _model.KeyStorePath = setPath;
                        return true;
                    }
                }
            }
            
            return false;
        }




        #endregion

        #region OtherCommands
        
        private void RemoveOldAdjustSignatureFiles()
        {
            // 删除无用的 Adjust 文件
            AdjustFileCleaner.RemoveOldSignatureFiles();        
        }

        private void Add16KbFilesForAndroid()
        {
            Guru16KbHelper.Apply();
        }

        #endregion
    }


    #region EditorModel
    
    [Serializable]
    internal class SDKMgrModel: EaseConfigFile
    {
        private const string ModelFileName = "guru_sdkmgr.settings";
        private const string ModelDirName = "guru/editor";
        private static string ModelDirPath => Path.GetFullPath($"{Application.dataPath.Replace("Assets", "ProjectSettings")}/{ModelDirName}");
        private static string ModelFilePath => Path.GetFullPath($"{ModelDirPath}/{ModelFileName}");


        private string _pushIconPath = "push_icon_path";
        private string _pushIconPathValue;
        public string PushIconPath
        {
            get => _pushIconPathValue;
            set
            {
                _pushIconPathValue = value;
                Set(_pushIconPath, _pushIconPathValue);
            }
        }
        
        private string _keyStorePath = "keystore_path";
        private string _keyStorePathValue;
        public string KeyStorePath
        {
            get => _keyStorePathValue;
            set
            {
                _keyStorePathValue = value;
                Set(_keyStorePath, _keyStorePathValue);
            }
        }


        public SDKMgrModel()
        {
        }

        public SDKMgrModel(string filePath)
        {
            ReadFile(ModelFilePath);

            _pushIconPathValue = Get(_pushIconPath);
            _keyStorePathValue = Get(_keyStorePath);
        }



        public static SDKMgrModel Load()
        {
            return new SDKMgrModel(ModelFilePath);
        }
        
    }

    #endregion
    
    
}