using System;
using AppLovinMax.Scripts.IntegrationManager.Editor;
using UnityEditor;
using UnityEngine;
using EUI= Guru.EasyGUILayout;
using Network = AppLovinMax.Scripts.IntegrationManager.Editor.Network;

namespace Guru.Editor.Max
{
    public class GuruMaxIntegrationManager: EditorWindow
    {
        private static GuruMaxIntegrationManager _currentWindow;
        private static Vector2 _miniSize = new Vector2(600, 800);
        private AppLovinSettings _settings;

        private static GUIStyle _labelBoldStyle;

        public static GUIStyle LabelBoldStyle {
            get {
                if (_labelBoldStyle == null)
                {
                    _labelBoldStyle = new GUIStyle(EditorStyles.label)
                    {
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 20
                    };
                }

                return _labelBoldStyle;
            }
        }
        
        private string _maxSdkKey;
        private string _admobAndroidId;
        private string _admobIOSId;
        private bool _qualityServiceEnabled = true;
        private bool _setAttributionReportEndpoint = true;
        private bool _addApsSkAdNetworkIds = true;
        private bool _isDirty;
        
        //------- AppLovinData --------
        private AppLovinEditorCoroutine loadDataCoroutine;
        private PluginData pluginData;
        
        #region 生命周期

        /// <summary>
        /// 打开窗体
        /// </summary>
        public static void Open()
        {
            if (_currentWindow != null)
            {
                _currentWindow.Close();
            }
            _currentWindow = GetWindow<GuruMaxIntegrationManager>();
            if (_currentWindow != null)
            {
                _currentWindow.minSize = _miniSize;
                _currentWindow.Show();
            }
        }


        private void Awake()
        {
            this.titleContent = new GUIContent("Guru.Max");
        }
        

        /// <summary>
        /// 窗体激活
        /// </summary>
        private void OnEnable()
        {
            LoadSettings();
            _isDirty = false;

            LoadPluginData();
        }

        private void OnDisable()
        {
            CheckSaveData(true);
            
            if (loadDataCoroutine != null)
            {
                loadDataCoroutine.Stop();
                loadDataCoroutine = null;
            }
        }

        #endregion


        #region GUI

        
        private void OnGUI()
        {

            GUI_Title();
            GUILayout.Space(4);
            GUI_MaxParamsSettings();
            GUILayout.Space(8);
            GUI_DrawMediatedNetworks();
#if MAX_DEV
            GUILayout.Space(8);
            GUI_DebugMenu();
#endif
            CheckSaveData();
        }


        private float _timePassed = 0;
        private void CheckSaveData(bool force = false)
        {
            _timePassed += Time.deltaTime;

            if (force || _timePassed >= 2 )
            {
                if ( force || _isDirty)
                {
                    SaveSettings();
                    _isDirty = false;
                }
                _timePassed= 0;
            }
        }


        private void GUI_Title()
        {
            GUILayout.Space(10);
            EUI.Label("Guru Max Manager", 0, 24, FontStyle.Bold, TextAnchor.MiddleCenter);
            GUILayout.Space(4);
            EUI.Label($"Version: {GuruMaxSdkAPI.Version}", fontSize:12, anchor:TextAnchor.MiddleCenter);
            GUILayout.Space(16);
        }


        #endregion

        #region DataSave

        private void LoadSettings()
        {
            _settings = GuruMaxSdkAPI.LoadOrCreateAppLovinSettings();
            if (null != _settings)
            {
                _maxSdkKey = _settings.SdkKey;
                _admobAndroidId = _settings.AdMobAndroidAppId;
                _admobIOSId = _settings.AdMobIosAppId;
                _qualityServiceEnabled = _settings.QualityServiceEnabled;
                _setAttributionReportEndpoint = _settings.SetAttributionReportEndpoint;
                _addApsSkAdNetworkIds = _settings.AddApsSkAdNetworkIds;
            }
            else
            {
                Debug.LogError("[GuruMax] Load AppLovinSettings failed...");
                if(EditorUtility.DisplayDialog("Guru Max", "Can't find AppLovinSettings in project, Something is wrong.", "OK"))
                {
                    Close();
                }
            }
        }



        private void SaveSettings()
        {
            if (null != _settings)
            {
                _settings.SdkKey = _maxSdkKey;
                _settings.AdMobAndroidAppId = _admobAndroidId;
                _settings.AdMobIosAppId = _admobIOSId;
                _settings.QualityServiceEnabled = _qualityServiceEnabled;
                _settings.SetAttributionReportEndpoint = _setAttributionReportEndpoint;
                _settings.AddApsSkAdNetworkIds = _addApsSkAdNetworkIds;
                
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssetIfDirty(_settings);
                
                Debug.Log("[GuruMax] AppLovinSettings Saved.");
            }
        }


        #endregion
        
        #region Params Settings


        private void GUI_MaxParamsSettings()
        {
            EditorGUILayout.LabelField("AppLovin Settings", LabelBoldStyle);
            GUILayout.Space(4);
            string indentStr = "  ";
            
            float label_width = 160;
            EUI.Label($"{indentStr}[ Keys for Max ]");
            //----------- MAX SDK KEY -----------
            EUI.BoxLineItem($"{indentStr}AppLovin SDK Key", label_width, contents: () =>
            {
                EUI.Text(_maxSdkKey, value =>
                {
                    _isDirty = true;
                    _maxSdkKey = value;
                });
            });
            
            GUILayout.Space(8);
            EUI.Label($"{indentStr}[ Ids for AdMob ]");
            EUI.BoxLineItem($"{indentStr}Google App ID (Android)", label_width, contents: () =>
            {
                EUI.Text(_admobAndroidId, value =>
                {
                    _isDirty = true;
                    _admobAndroidId = value;
                });
            });

            EUI.BoxLineItem($"{indentStr}Google App ID (iOS)", label_width, contents: () =>
            {
                EUI.Text(_admobIOSId, value =>
                {
                    _isDirty = true;
                    _admobIOSId = value;
                });
            });
            GUILayout.Space(8);
            
            
            EUI.Label($"{indentStr}[ Default: {GuruMaxSdkAPI.DefaultQualityServiceEnabled} ]");
            EUI.BoxLineItem($"{indentStr}Quality Service Enabled", label_width, contents: () =>
            {
                EUI.Toggle(_qualityServiceEnabled, value =>
                {
                    _isDirty = true;
                    _qualityServiceEnabled = value;
                });
            });
            GUILayout.Space(8);
            
            EUI.Label($"{indentStr}[ Default: {GuruMaxSdkAPI.DefaultAttributionReportEndpoint} ]");
            EUI.BoxLineItem($"{indentStr}Attribution Report Endpoint(iOS)", label_width, contents: () =>
            {
                EUI.Toggle(_setAttributionReportEndpoint, value =>
                {
                    _isDirty = true;
                    _setAttributionReportEndpoint = value;
                });
            });
            GUILayout.Space(8);
            
            EUI.Label($"{indentStr}[ Default: {GuruMaxSdkAPI.DefaultAddApsSkAdNetworkIds} ]");
            EUI.BoxLineItem($"{indentStr}Add Aps SkAdNetwork Ids(iOS)", label_width, contents: () =>
            {
                EUI.Toggle(_addApsSkAdNetworkIds, value =>
                {
                    _isDirty = true;
                    _addApsSkAdNetworkIds = value;
                });
            });
            GUILayout.Space(8);

        }


        #endregion

        #region AppLovinData
        
        
        private void LoadPluginData()
        {
            loadDataCoroutine = AppLovinEditorCoroutine.StartCoroutine(
                AppLovinIntegrationManager.Instance.LoadPluginData( data => {
                    if (data != null)
                    {
                        pluginData = data;
                        _showNetworks = true;
                    }
                }));
        }

        private bool _showNetworks = false;
        private Vector2 scrollPos;
        /// <summary>
        /// 绘制接入的各种Network
        /// </summary>
        private void GUI_DrawMediatedNetworks()
        {
            
            float label_width = 160;
            EditorGUILayout.LabelField("Mediated Networks", LabelBoldStyle);


            _showNetworks = EditorGUILayout.Foldout(_showNetworks, "Installed Networks" + (pluginData == null ? "  (loading...)": ""));

            if (_showNetworks)
            {
                if (pluginData == null || pluginData.MediatedNetworks == null)
                {
                    EditorGUILayout.LabelField("Loading...", new GUIStyle("box"), GUILayout.Width(this.minSize.x));
                }
                else
                {
                
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        EditorGUI.indentLevel++;
                        DrawMaxTitle("Network", "Android", "iOS");
                        // scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(320));
                        GUILayout.Space(6);
                        Network network = null;
                        for (int i =0; i < pluginData.MediatedNetworks.Length; i++)
                        {
                            network = pluginData.MediatedNetworks[i];
                            if ( null != network && network.CurrentVersions != null &&
                                 !string.IsNullOrEmpty(network.CurrentVersions.Unity))
                            {
                                DrawMaxNetwork(network);
                                GUILayout.Space(2);
                            }
                        }
                        // EditorGUILayout.EndScrollView();
                        EditorGUI.indentLevel--;
                    }
                }
            }

        }


        private void DrawMaxTitle(string name, string androidVersion, string iosVersion)
        {
            var st = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 22
            };

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(name, st);
                // GUILayout.Space(2);
                EditorGUILayout.LabelField(androidVersion, st);
                // GUILayout.Space(2);
                EditorGUILayout.LabelField(iosVersion, st);
            }
        }

        private void DrawMaxNetwork(Network network)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(network.DisplayName);
                // GUILayout.Space(2);
                EditorGUILayout.LabelField(network.CurrentVersions.Android ?? "...");
                // GUILayout.Space(2);
                EditorGUILayout.LabelField(network.CurrentVersions.Ios ?? "...");
            }
        }


        #endregion

        #region Debug


        private bool _showMaxIntegrateManager = false;
        private void GUI_DebugMenu()
        {
            EditorGUILayout.LabelField("Debug Menu", LabelBoldStyle);
            GUILayout.Space(2);

            bool val = EditorGUILayout.Toggle("Show Max Menu", _showMaxIntegrateManager);
            if (val != _showMaxIntegrateManager)
            {
                _showMaxIntegrateManager = val;
                GuruMaxSdkAPI.SetMaxMenuActive(val);
            }



        }

        #endregion
    }
}