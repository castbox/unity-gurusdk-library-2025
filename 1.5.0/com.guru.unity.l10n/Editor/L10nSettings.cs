
namespace Guru
{
     using System;
     using System.Collections.Generic;
     using System.IO;
     using System.Linq;
     using UnityEngine;
     internal class L10nIO
     {
          /// <summary>
          /// 配置地址
          /// </summary>
          private const string L10N_SETTINGS_NAME = "l10n_settings.json";
          private static string SettingsPath
          {
               get => Path.GetFullPath($"{Application.dataPath}/../ProjectSettings/{L10N_SETTINGS_NAME}");
          }
          
          /// <summary>
          /// 加载文件
          /// </summary>
          /// <returns></returns>
          public static L10nSettings LoadOrCreate()
          {
               if (File.Exists(SettingsPath))
               {
                    var raw = File.ReadAllText(SettingsPath);
                    try
                    {
                         return JsonUtility.FromJson<L10nSettings>(raw);
                    }
                    catch (Exception e)
                    {
                         Debug.LogError(e);
                    }
               }
               return new L10nSettings();
          }

          /// <summary>
          /// 保存文件
          /// </summary>
          /// <param name="settings"></param>
          public static void Save(L10nSettings settings)
          {
               var json = JsonUtility.ToJson(settings);
               File.WriteAllText(SettingsPath, json);
          }
     }


     /// <summary>
     /// L10n 配置
     /// </summary>
     [Serializable]
     public class L10nSettings
     {
          public const string DefaultSheetId = "1Uq3458LWTTSCQhjC4T3peRHKuC8_FbGj6Z2LWJrx0Zs";
          public static readonly string DefaultTableName = "default_table";
          
          public string sheet_id; // 表格ID
          public string table_name; // 表格名称
          public string update_date; // 更新时间
          public string proxy_host; // 代理地址
          public string proxy_port; // 代理端口
          public bool proxy_enable; // 是否启用代理

          public List<GoogleSheetData> sheets; // 独立翻译表格

          public string GetProxyUrl()
          {
               if(!proxy_enable) return ""; // Proxy 不可用
               
               if (!string.IsNullOrEmpty(proxy_host) && !string.IsNullOrEmpty(proxy_port))
               {
                    return $"http://{proxy_host}:{proxy_port}";
               }
               return "";
          }

          public L10nSettings()
          {
               sheet_id = DefaultSheetId;
               table_name = DefaultTableName; // 表格名称
               proxy_host = "127.0.0.1";
               proxy_port = "7890";
               proxy_enable = false;
               
               sheets = new List<GoogleSheetData>(); // 独立表格列表
               UpdateDate();
          }

          public void UpdateDate()
          {
               update_date = DateTime.Now.ToLocalTime().ToString("g");
          }
          
          public static L10nSettings LoadOrCreate() => L10nIO.LoadOrCreate();

          public void Save()
          {
               this.UpdateDate();
               L10nIO.Save(this);  
          }

          /// <summary>
          /// 添加 GoogleSheet 表格数据
          /// </summary>
          /// <param name="url"></param>
          /// <param name="tableName"></param>
          public bool AddGoogleSheet(string url, string tableName)
          {
               if(sheets == null) sheets = new List<GoogleSheetData>();
               var f = sheets.FirstOrDefault(c => c.url == url);
               if (null == f)
               {
                    sheets.Add(GoogleSheetData.Create(url, tableName));
                    Save();
                    return true;
               }
               return false;
          }

          public bool IsSheetsEmpty => sheets == null || sheets.Count == 0;
     }

     /// <summary>
     /// 独立表格配置
     /// </summary>
     [Serializable]
     public class GoogleSheetData
     {
          public string url;
          public string sheet_id;
          public string table_name;
          
          /// <summary>
          /// 创建 Google Sheet ID
          /// </summary>
          /// <param name="url"></param>
          /// <param name="table"></param>
          /// <returns></returns>
          public static GoogleSheetData Create(string url, string tableName)
          {

               GoogleSheetData data = new GoogleSheetData();
               
               data.url = url;
               
               // https://docs.google.com/spreadsheets/d/1A3Swcu1Y4Bm58OBNIuP1i2l1V08TFMt0GXExEzrLe_g/edit#gid=1598741415
               var tmp = url.Replace(GuruI2LocAPI.GoogleSheetHeader, "");
               data.sheet_id = tmp.Substring(0, tmp.IndexOf('/'));
               data.table_name = tableName;
               return data;
          }
     }
     
}