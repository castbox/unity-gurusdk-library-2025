using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UnityEditor;
using UnityEngine;
using API = Guru.GuruI2LocAPI;

namespace Guru
{
     
     public class L10nEditor : EditorWindow
     {
          public static readonly int WindowWidth = 550;
          public static readonly int WindowHeight = 800;
          
          
          private static L10nEditor _instance;

          #region 菜单入口

          [MenuItem("Guru/Localization")]
          private static void ShowWindow()
          {
               if (_instance != null) _instance.Close();
               
               _instance = CreateInstance<L10nEditor>();
               _instance.titleContent = new GUIContent("L10N");
               _instance.minSize = new Vector2(WindowWidth, WindowHeight);
               _instance.Show();
          }


          [MenuItem("Tools/关闭进度条")]
          private static void CloseProgressBar()
          {
               EditorUtility.ClearProgressBar();
          }

          #endregion
          
          private L10nSettings _settings;
          private string _sheetId;
          private string _tableName;
          private string _updateDate;
          private List<string> _unsupported;
          private bool _foldoutProxyFlag = false;

          #region 初始化

          private void OnEnable()
          {
               _settings = L10nSettings.LoadOrCreate();
               _sheetId = _settings.sheet_id;
               _tableName = _settings.table_name;
               _updateDate = _settings.update_date;

               API.CheckEnvironment(); //  默认检查环境配置
               _unsupported = API.CheckUnsupported(); // 检查不支持的语言
          }

          private void SaveSettings()
          {
               _settings.Save();
               _updateDate = _settings.update_date;
          }


          #endregion
          
          #region GUI Common

          private void OnGUI()
          {
               GUI_Title();
               GUI_Basic();
               GUI_Controls();
               GUI_Extras(); // 扩展数据
          }

          /// <summary>
          /// Box 风格图框
          /// </summary>
          /// <param name="label"></param>
          /// <param name="onLayout"></param>
          /// <param name="anchor"></param>
          private static void Box(string label, Action onLayout, TextAnchor anchor = TextAnchor.MiddleLeft)
          {
               GUIStyle s = new GUIStyle("Box");
               s.alignment = anchor;
               GUILayout.BeginHorizontal(s);
               GUILayout.Label(label, GUILayout.Width(60));
               onLayout?.Invoke();
               GUILayout.EndHorizontal();
          }
          
          /// <summary>
          /// 颜色调节
          /// </summary>
          /// <param name="color"></param>
          /// <param name="onLayout"></param>
          private static void Color(Color color, Action onLayout)
          {
               var c = UnityEngine.GUI.color;
               UnityEngine.GUI.color = color;
               onLayout?.Invoke();
               UnityEngine.GUI.color = c;
          }



          private static void ColorButton(Color color, string label, Action onButton,  int width = 0, int height = 0)
          {
               Color(color, () =>
               {
                    List<GUILayoutOption> options = new List<GUILayoutOption>();
                    if(width> 0) options.Add(GUILayout.Width(width));
                    if(height > 0 ) options.Add(GUILayout.Height(height));
                    if(GUILayout.Button(label, options.ToArray()))
                    {
                         onButton?.Invoke();
                    }
               });
          }



          #endregion
          
          #region GUI属性

          private void GUI_Title()
          {
               var s = new GUIStyle("Box");
               s.fontSize = 30;
               s.alignment = TextAnchor.MiddleCenter;
               s.stretchWidth = true;
               GUILayout.Space(4);
               GUILayout.Label("Guru L10N", s, GUILayout.Width(WindowWidth), GUILayout.Height(80));
               GUILayout.Space(4);
               GUILayout.Label($"Version: {API.Version}", GUILayout.Width(WindowWidth));
               GUILayout.Space(4);
               GUILayout.BeginHorizontal("Box");
               
               ColorButton(new Color(0.9f, 0.3f, 0.0f), "刷新工具",() =>
               {
                    ResetToolEnv();
               }, height:30);
               
               ColorButton(new Color(0.4f, 1f, 0.0f), "线上表格",() =>
               {
                    OpenGoogleSheet();
               }, height:30);
               
               if (_unsupported == null)
               {
                    if (GUILayout.Button("guru 配置未拉取"))
                    {
                         _unsupported = API.CheckUnsupported(); // 检查不支持的语言
                    }
               }
               else if (_unsupported.Count > 0)
               {
                    ColorButton(new Color(1f, 1f, 0.24f), "不兼容语言",() =>
                    {
                         EditorUtility.DisplayDialog("发现不兼容语言", $"{string.Join(",",_unsupported)}", "OK");
                    }, height:30);
               }
               else
               {
                    // 全部兼容
               }
               
               ColorButton(new Color(0.8f, 0.2f, 0.6f), "工作目录", GuruI2LocAPI.OpenWorkingDir, height: 30);
               GUILayout.EndHorizontal();
               GUILayout.Space(2);

               s = new GUIStyle("Box");
               s.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 0.7f);
               GUILayout.Label("►  项目初始化时需要执行", s);
               GUILayout.BeginHorizontal("Box");
               ColorButton(new Color(1f, 0.8f, 0.2f), "补全默认语言", SetupRecommendsLanguages, height: 30);
               GUILayout.EndHorizontal();
               GUILayout.Space(10);
          }

          /// <summary>
          /// 基础参数
          /// </summary>
          private void GUI_Basic()
          {
               // ------ Sheet ID ------
               GUILayout.Label("[ 主表ID ]");
               Box("Sheet ID", () =>
               {
                    _sheetId = EditorGUILayout.TextField(_sheetId);
                    if (_sheetId != _settings.sheet_id)
                    {
                         if (GUILayout.Button("✔"))
                         {
                              _settings.sheet_id = _sheetId;
                              SaveSettings();
                         }

                         if (GUILayout.Button("✘"))
                         {
                              _sheetId = _settings.sheet_id;
                         }

                    }

               });
               // ------ Table Name ------
               string hint = "[ 翻译表格名称 ]";
               if (_tableName == L10nSettings.DefaultTableName) hint = "[ 项目表格名称, 从GoogleSheet下方的Table名称处查询 ]";
               GUILayout.Label(hint);
               Box("Table", () =>
               {
                    _tableName = EditorGUILayout.TextField(_tableName);
                    if (_tableName != _settings.table_name)
                    {
                         if (GUILayout.Button("✔"))
                         {
                              _settings.table_name = _tableName;
                              SaveSettings();
                         }

                         if (GUILayout.Button("✘"))
                         {
                              _tableName = _settings.table_name;
                         }

                    }
               });
               // ------- Date ---------
               GUILayout.Label("[ 更新日期 ]");
               Box("Date", () =>
               {
                    GUILayout.Label(_updateDate);
               });
               
               // ------- Proxy ---------
               GUILayout.Label("[ 代理配置 ]");
               Box("Proxy", () =>
               {
                    // GUILayout.Label("启用代理", GUILayout.Width(80));
                    bool proxyFlag = EditorGUILayout.ToggleLeft("启用代理", _settings.proxy_enable, GUILayout.Width(90));
                    if (_settings.proxy_enable != proxyFlag)
                    {
                         _settings.proxy_enable = proxyFlag;
                         SaveSettings();
                    }
                    
                    if (_settings.proxy_enable)
                    {
                         GUILayout.Label("HOST:");
                         var host = EditorGUILayout.TextField(_settings.proxy_host);
                         if (host != _settings.proxy_host)
                         {
                              _settings.proxy_host = host;
                              SaveSettings();
                         } 
                         GUILayout.Space(10);
                         
                         GUILayout.Label("PORT:");
                         var proxy = EditorGUILayout.TextField(_settings.proxy_port);
                         if (proxy != _settings.proxy_port)
                         {
                              _settings.proxy_port = proxy;
                              SaveSettings();
                         }
                         
                    }
               }); 
          }


          private bool ShowSyncDailog(bool success, string msg = "")
          {
               string title = success ? "多语言数据同步成功" : "多语言数据同步失败";
               if (string.IsNullOrEmpty(msg)) msg = success ? "已成功更新" : "更新失败, 请稍后再试";
               return EditorUtility.DisplayDialog(title, msg, "OK");
          }

          private void GUI_Controls()
          {
               var s = new GUIStyle("Box");
               // s.alignment = TextAnchor.MiddleCenter;
               // s.fontStyle = FontStyle.Bold;
               s.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 0.7f);

            
               GUILayout.Space(10);
               GUILayout.Label("►  本地数据有更新时执行", s);
               GUILayout.Space(5);
               // 导出所有的翻译
               ColorButton(new Color(0.2f, 1f, 0.88f), "导出并上传", 
               () =>
               {
                    /*API.ExportAndSync(_settings.sheet_id, _settings.table_name, success =>
                    {
                         if (ShowSyncDailog(success))
                         {
                              if(success) OpenGoogleSheet();
                         }
                    });*/

                    API.ExportCSV(); // 先导出文件
                    ExecuteAction(L10NAction.sync_translate,success =>
                    {
                         if (ShowSyncDailog(success))
                         {
                              if(success) OpenGoogleSheet();
                         }
                    }); // 调用命令
               }, 
               height:60);
               
               GUILayout.Space(10);
               GUILayout.Label("►  请与 ASO 确认后, 再导入线上数据",s);
               GUILayout.Space(5);
               // 导入所有的翻译
               ColorButton(new Color(1f, 0f, 0.46f), "下载并导入", 
               () =>
               {
                    /*API.SyncAndImport(_settings.sheet_id, _settings.table_name, success =>
                    {
                         if (ShowSyncDailog(success, success ? "已成功导入多语言数据" : "传输或导入有问题, 请稍后再试."))
                         {
                              if (success)
                              {
                                   Selection.activeObject = Resources.Load<UnityEngine.Object>("I2Languages");
                              }
                         }
                    }); */  
                    
                    ExecuteAction(L10NAction.sync, success =>
                    {
                         
                         if (ShowSyncDailog(success, success ? "已成功导入多语言数据" : "传输或导入有问题, 请稍后再试."))
                         {
                              if (success)
                              {
                                   if (API.ImportCSV()) // 导入CSV
                                   {
                                        Selection.activeObject = Resources.Load<UnityEngine.Object>("I2Languages");
                                        API.BackupCurFile(); // 刷新本次的翻译副本
                                   }
                                   else
                                   {
                                        Debug.LogError($"未找到多语言配置文件...");
                                   }
                              }
                         }
                    }); // 调用命令
                    
                    
               }, 
               height:60);
          }
          
          /// <summary>
          /// 调用翻译接口
          /// </summary>
          /// <param name="isSync"></param>
          /// <param name="callback"></param>
          private void ExecuteAction(L10NAction acton, Action<bool> callback = null)
          {
               API.CallSyncCmd(_settings.sheet_id, _settings.table_name, acton, _settings.GetProxyUrl(), callback); // 调用命令
          }

          
          //----- 扩展数据 -----
          private string _newSheetUrl;
          private string _newTableName;
          private Vector2 _sheetScrollPos;
          
          private void GUI_Extras()
          {
               bool isDirty = false;
               
               var s = new GUIStyle("Box");
               s.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 0.7f);

            
               GUILayout.Space(20);
               GUILayout.Label("►  [扩展数据] (直接粘贴表地址即可, 以及表格名称)", s);
               GUILayout.Space(5);
               
               _newSheetUrl = EditorGUILayout.TextField("Sheet URL",_newSheetUrl);
               _newTableName = EditorGUILayout.TextField("Table",_newTableName);
               
               // 导出所有的翻译
               ColorButton(new Color(0.2f, 1f, 0.88f), "新增", 
               () =>
               {
                    string title = "添加GoogleSheet";
                    string msg = "";    
                    if (_settings.AddGoogleSheet(_newSheetUrl, _newTableName))
                    {
                         _newSheetUrl = "";
                         _newTableName = "";
                         isDirty = true;
                         msg = "添加数据成功";
                    }
                    else
                    {
                         _newSheetUrl = "";
                         _newTableName = "";
                         msg = "添加数据失败";
                    }

                    EditorUtility.DisplayDialog(title, msg, "好的");
               },
               height:24);
               
               if (!isDirty && !_settings.IsSheetsEmpty)
               {
                    GUILayout.Space(15);
                    s.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("►  [独立翻译表格数据列表] (仅负责翻译,后继操作请手动完成)", s);
                    GUILayout.Space(10);

                    // 显示Sheet列表
                    _sheetScrollPos = GUILayout.BeginScrollView(_sheetScrollPos, GUILayout.Height(400));
                    for (int i = 0; i < _settings.sheets.Count; i++)
                    {
                         GUI_OneExtraSheet(_settings.sheets[i]);
                    }
                    
                    GUILayout.EndScrollView();
               }
               else
               {
                    GUILayout.Label("\t暂无数据.....",s);
               }
               
          }


          private void GUI_OneExtraSheet(GoogleSheetData data)
          {
               var h = 20;
               var s = new GUIStyle("Box");
               s.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 0.7f);
               
               
               
               GUILayout.BeginHorizontal(s);
               GUILayout.Space(15);
               GUILayout.Label($"+ [{data.table_name}]: {data.sheet_id}");
               
               // 导出所有的翻译
               ColorButton(new Color(0.9f, 0.3f, 0.0f), "翻译", 
               () => 
               {
                    OpenTranslateCommand(data,success =>
                    {
                         if (ShowSyncDailog(success))
                         {
                              if(success) OpenGenTable(data);
                         }
                    }); // 调用命令
               }, 
               height: h);
               
               // 打开链接
               ColorButton(new Color(0.4f, 1f, 0.0f), "前往", 
                    ()=> OpenGenTable(data), 
                    height: h);
               
               GUILayout.EndHorizontal();
               GUILayout.Space(4);
          }


          private void OpenTranslateCommand(GoogleSheetData data, Action<bool> callback = null)
          {
               API.CallSyncCmd(data.sheet_id, data.table_name, L10NAction.translate, _settings.GetProxyUrl(), callback); // 调用命令
          }

          private void OpenGenTable(GoogleSheetData data)
          {
               Application.OpenURL(data.url);
          }

          #endregion

          #region 其他接口

          private void OpenGoogleSheet()
          {
               var url = $"{API.GoogleSheetHeader}{_settings.sheet_id}";
               Application.OpenURL(url);
          }
          
          /// <summary>
          /// 重置工具环境
          /// </summary>
          private void ResetToolEnv()
          {
               API.ResetEnvironment(); //  默认检查环境配置
          }



          #endregion
          
          #region 默认语言支持

          /// <summary>
          /// 设置默认语言
          /// </summary>
          private void SetupRecommendsLanguages()
          {
               API.SetupRecommendedLanguages(out var allCount, out var missionCount, out var asset);
               if (EditorUtility.DisplayDialog("语言处理完毕", $"共添加了{missionCount}个语言", "OK"))
               {
                    Debug.Log($"--- 全部语言: <color=#88ff00>{allCount}</color>");
                    Selection.activeObject = asset;
               }
          }



          #endregion
     }



}
