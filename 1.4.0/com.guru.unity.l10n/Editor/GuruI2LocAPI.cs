
namespace Guru
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using System.Threading.Tasks;
    using Action = System.Action;
    using Debug = UnityEngine.Debug;
    using Object = UnityEngine.Object;
    using I2.Loc;

    
    public enum L10NAction
    {
        sync,
        sync_translate,
        translate
    }
    
    
    /// <summary>
    /// I2 语言扩展功能
    /// </summary>
    public class GuruI2LocAPI
    {
        // API版本
        public static readonly string Version = "0.0.2";
        
        // Google Sheet Prefix
        public const string GoogleSheetHeader = "https://docs.google.com/spreadsheets/d/";
        
        private const char DEFAULT_SEPARATOR = ',';
        private const char DEFAULT_LINE_CHANGE = '\n';
        private const string LOCALIZATION_FILENAME = "localization.csv";
        private const string BACKUP_FILENAME = "backup.csv";
        private const string SYNC_COMMAND = "sync.command";
        private const string SYNC_BAT = "sync.bat";
        private const string PARAMS_FILE = "params";
        private const string DEFAULT_LOG_NAME = "log.txt";
        private const string L10N_EXE = "l10n";
        
        
        private const string ACTION_SYNC = "sync";
        private const string ACTION_SYNC_TRANSLATE = "sync_translate";
        private const string ACTION_TRANSLATE = "translate";
        

        private static LanguageSourceAsset _asset;
        private static bool _showProgress = false;
        private static bool _isExternalProject = false;

        private static string[] _guruConfigRepos = new string[]
        {
            "git@guru-upm:castbox/guru_config.git",
            "git@github.com:castbox/guru_config.git",
        };

        
        public static LanguageSourceAsset LoadI2Asset() => Resources.Load<LanguageSourceAsset>("I2Languages");
        
        public static LanguageSourceData GetSourceData()
        {
            _asset = LoadI2Asset();
            return _asset?.mSource ?? null;
        }
        
        /// <summary>
        /// 工作路径
        /// 需要手动建立
        /// </summary>
        public static string WorkingDir => Path.GetFullPath($"{Application.dataPath}/../Library/guru_l10n");
        private static string OldStorageDir => Path.GetFullPath($"{Application.dataPath}/../../.guru/l10n");
        private static string NewStorageDir => Path.GetFullPath($"{Application.dataPath}/../ProjectSettings/guru/l10n");
        /// <summary>
        /// 存储文件路径 (随项目备份)
        /// 自动建立
        /// </summary>
        public static string StorageDir
        {
            get
            {
                CleanOldStorageDir();
                if (!Directory.Exists(NewStorageDir)) Directory.CreateDirectory(NewStorageDir);
                return NewStorageDir;
            }
        }


        private static string _envDir = ""; 

        /// <summary>
        /// 命令行工具
        /// </summary>
        public static string EnvDir
        {
            get
            {
                if (string.IsNullOrEmpty(_envDir))
                {
                    var guids = AssetDatabase.FindAssets($"{nameof(GuruI2LocAPI)} t:Script");
                    if (guids != null)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        _envDir = $"{Directory.GetParent(path).FullName}";
                    }
                    else
                    {
                        _envDir = $"Packages/com.guru.unity.sdk.core/Runtime/GuruL10n/Editor";
                    }
                }
                return _envDir;
                // return Path.GetFullPath($"{Application.dataPath}/Guru/GuruL10n/Editor/cmd");
            }
        }


        public static string CmdDir => $"{EnvDir}/cmd";
        public static string FilesDir => $"{EnvDir}/files";
        

        /// <summary>
        /// Guru 翻译文件的路径
        /// </summary>
        /// <returns></returns>
        public static string LocalizationFilePath => Path.GetFullPath($"{WorkingDir}/{LOCALIZATION_FILENAME}");
        /// <summary>
        /// 备份文件路径
        /// </summary>
        /// <returns></returns>
        public static string BackupFilePath => Path.GetFullPath($"{StorageDir}/{BACKUP_FILENAME}");

        /// <summary>
        /// 主文件缓存路径
        /// </summary>
        public static string LocalizationCachePath => Path.GetFullPath($"{StorageDir}/{LOCALIZATION_FILENAME}");
        
        #region 文件导出
        
        
        /// <summary>
        /// 导出为 CSV 字符串
        /// </summary>
        /// <returns></returns>
        public static string ExportGuruCSVString()
        {
            var source = GetSourceData();
            if (null != source)
            {
                var csv = source.Export_CSV(null, DEFAULT_SEPARATOR);

                // if (csv.Contains("\r")) Debug.Log($"--- Has \\R");
                // if (csv.Contains("\n\r")) Debug.Log($"--- Has \\N\\R");
                
                // 仅修改Header行, 插入新的格式
                var lines = csv.Split(DEFAULT_LINE_CHANGE).ToList();
                var names = lines[0]
                    .Replace($"Key{DEFAULT_SEPARATOR}Type{DEFAULT_SEPARATOR}Desc{DEFAULT_SEPARATOR}", "")
                    .Split(DEFAULT_SEPARATOR);

                // 获取语言Code值
                List<string> codes = new List<string>(90);
                foreach (var name in names)
                {
                    var code = GetLanguageCode(source, name);
                    codes.Add(code.Replace("-", "_")); // 转化为 "_" 的形式
                }
                
                string header = $"Code{DEFAULT_SEPARATOR}--{DEFAULT_SEPARATOR}--{DEFAULT_SEPARATOR}";
                header = $"{header}{string.Join(DEFAULT_SEPARATOR.ToString(), codes)}";
                lines.Insert(1, header);

                string type, row;
                int i = lines.Count - 1;
                while (lines.Count > 1)
                {
                    if (i >= lines.Count)
                    {
                        Debug.Log($"Out of range: {i}/{lines.Count}");
                        break;
                    }

                    row = lines[i];
                    if (string.IsNullOrEmpty(row))
                    {
                        i--;
                        continue;
                    }

                    if (row.Contains(DEFAULT_SEPARATOR))
                    {
                        var t = row.Split(DEFAULT_SEPARATOR);
                        if (t.Length > 2)
                        {
                            type = t[1];
                            if (type == "Sprite")
                            {
                                lines.RemoveAt(i);
                                continue;
                            }
                        }
                    }
                    
                    i--;
                    if(i < 2) break;
                }
                
                return string.Join(DEFAULT_LINE_CHANGE.ToString(), lines);
            }

            return "";
        }
        
        /// <summary>
        /// 获取语言Code
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetLanguageCode(LanguageSourceData data, string name)
        {
            if(name.EndsWith("]") && name.Contains(" ["))
            {
                name = name.Substring(0, name.IndexOf(" [", StringComparison.Ordinal));
            }
            var ld = data.mLanguages.FirstOrDefault(c => c.Name == name);
            if (ld != null) return ld.Code;
            return "";
        }

        #endregion
        
        #region 文件导入


        /// <summary>
        /// 导入CSV文件
        /// </summary>
        /// <param name="filePath"></param>
        private static bool ImportGuruCsvFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                return ImportGuruCSVString(File.ReadAllText(filePath));
            }
            return false;
        }



        public static bool ImportGuruCSVString(string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                return false;
            }
            
            var asset = LoadI2Asset();
            LanguageSourceData source = asset?.mSource ?? null;

            if (source == null) return false;



            var lines = LocalizationReader.ReadCSV(csv);
            if (!lines[0][0].StartsWith("Key"))
            {
                return false;
            }
            
            if (string.IsNullOrEmpty(lines[1][0]))
            {
                lines.RemoveAt(1);
            }
   
            if (lines[1][0].StartsWith("Code"))
            {
                lines.RemoveAt(1);
            }
            
            if (source != null)
            {
                source.Import_CSV("", 
                    lines, 
                    eSpreadsheetUpdateMode.Merge); // 合并到老的文件内

                foreach (var l in source.mLanguages)
                {
                    if (string.IsNullOrEmpty(l.Code))
                    {
                        if (l.Name.Contains("\r") || l.Name.Contains("\n"))
                        {
                            l.Name = l.Name.Replace("\r", "").Replace("\n", "");
                            l.Code = FindLanguageCodeByName(l.Name);
                            if (string.IsNullOrEmpty(l.Code))
                            {
                                Debug.LogError($"Can't find language code ofr {l.Name}. Check data in [recommanded.json]({RecI2Languages.FilePath})");
                            }
                        }
                    }
                }
                
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                return true;
            }
            
            return false;
        }

        #endregion

        #region 文件备份


        public static void BackupFromData(string csv, string path = "")
        {
            if (string.IsNullOrEmpty(path)) path = BackupFilePath;
            File.WriteAllText(path, csv);
        }

        public static void BackupCurFile()
        {
            string csv = File.ReadAllText(LocalizationFilePath);
            BackupFromData(csv);
        }

        public static bool CopyFile(string source, string dest, bool overwrite = true)
        {
            if (File.Exists(source))
            {
                var dir = Directory.GetParent(dest);
                if (dir != null && !dir.Exists) dir.Create();
                if (overwrite && File.Exists(dest)) File.Delete(dest);
                FileUtil.CopyFileOrDirectory(source, dest);
                return true;
            }
            return false;
        }

        public static bool MoveFile(string source, string dest)
        {
            if (CopyFile(source, dest))
            {
                File.Delete(source);
                return true;
            }
            return false;
        }


        private static void CleanOldStorageDir()
        {
            if (!Directory.Exists(OldStorageDir)) return;
            if (!Directory.Exists(NewStorageDir)) Directory.CreateDirectory(NewStorageDir);
            string from = $"{OldStorageDir}/{BACKUP_FILENAME}";
            string to = $"{NewStorageDir}/{BACKUP_FILENAME}";
            File.Move(from, to);
            Directory.Delete(OldStorageDir, true);
        }


        #endregion
        
        #region 环境部署

        private static string[] MacEnvFiles = new string[]
        {
            L10N_EXE,
            SYNC_COMMAND,
        };
        
        private static string[] WinEnvFiles = new string[]
        {
            L10N_EXE,
            SYNC_BAT,
        };
        
        
        private static string[] EnvCopyFiles
        {
            get
            {
#if UNITY_EDITOR_OSX
                return MacEnvFiles;
#else
                return WinEnvFiles;
#endif
            }
        }
        
        /// <summary>
        /// 环境检查
        /// </summary>
        public static void CheckEnvironment()
        {
            var tarDir = WorkingDir;
            var sorDir = CmdDir;
            var tarDi = new DirectoryInfo(tarDir);
            if (!tarDi.Exists) tarDi.Create();
            if (tarDi.GetFiles().Length == 0)
            {
                Directory.CreateDirectory(tarDir);
                var di = new DirectoryInfo(sorDir);
                // var files = di.GetFiles( "*.*", SearchOption.AllDirectories);
                string from, to;
                // 部署执行文件
                foreach (var s in EnvCopyFiles)
                {
                    from = $"{sorDir}/{s}";
                    to = $"{tarDir}/{s}";
                    File.Copy(from, to);
                }
            }
            
            var cmd_file = $"{tarDir}/{L10N_EXE}";
            if (File.Exists(cmd_file))
            {
                var txt = File.ReadAllText(cmd_file);
#if UNITY_EDITOR_WIN
                txt = txt.Replace("is_win = False", "is_win = True");
#endif
                int idx = _isExternalProject ? 1 : 0;
                var repo = _guruConfigRepos[idx];
                txt = txt.Replace("$REPO", repo);

                File.WriteAllText(cmd_file, txt);
            }
            else
            {
                Debug.LogError($"--- Can't find cmd file: {cmd_file}");    
            }
            
            CleanOldStorageDir(); // 清理陈旧的文件
            // Debug.Log($"<color=#88ff00>--- Env cleaned and rebuild! ---</color>");
        }
        
        /// <summary>
        /// 重置环境
        /// </summary>
        public static void ResetEnvironment()
        {
            
            bool isMoved = MoveFile(LocalizationFilePath, LocalizationCachePath);
            if (Directory.Exists(WorkingDir))
            {
                UnityEditor.FileUtil.DeleteFileOrDirectory(WorkingDir);
                Debug.Log($">>> Delete: {WorkingDir}");
            }
            
            CheckEnvironment();
            
            if (isMoved)
            {
                MoveFile(LocalizationCachePath, LocalizationFilePath);// Recover Loc
            }
        }

        /// <summary>
        /// 设置命令参数
        /// </summary>
        /// <param name="parameters"></param>
        private static void SaveParamsFile(List<string> parameters)
        {
            string exts = "";
#if UNITY_EDITOR_WIN
            exts = ".bat";
#endif
            string path = $"{WorkingDir}/{PARAMS_FILE}{exts}";
            File.WriteAllLines(path, parameters.ToArray());
        }

        private static void BuildParams(string sheetId, string tableName, string action, 
            string fileName = LOCALIZATION_FILENAME, string proxyUrl = "", string logName="")
        {
            if(string.IsNullOrEmpty(logName)) logName = DEFAULT_LOG_NAME;
            
            var list = new List<string>(10);
            
            // ------- PROXY -----------
            if (!string.IsNullOrEmpty(proxyUrl))
            {
                list.Add(BuildEnvKVP("HTTP_PROXY", proxyUrl));
                list.Add(BuildEnvKVP("HTTPS_PROXY", proxyUrl));
            }
            // ------- PARAMS -----------
            list.Add(BuildEnvKVP("ACTION", action));
            list.Add(BuildEnvKVP("SHEET_ID", sheetId));
            list.Add(BuildEnvKVP("TABLE_NAME", tableName));
            list.Add(BuildEnvKVP("FILE", fileName));
            list.Add(BuildEnvKVP("LOG", logName));

            SaveParamsFile(list);
        }

        private static string BuildEnvKVP(string key, string value)
        {
            string prefix = "";
#if UNITY_EDITOR_WIN
            prefix = "set ";
#endif
            return $"{prefix}{key}={value}";
        }


        private static void CheckMainLocalizationFile()
        {
            if (File.Exists(LocalizationFilePath)) return;

            if (File.Exists(BackupFilePath))
            {
                CopyFile(BackupFilePath, LocalizationFilePath);
                return;
            }

            if (File.Exists(LocalizationCachePath))
            {
                CopyFile(LocalizationCachePath, LocalizationFilePath);
                return;
            }
            
            ExportCSV();
        }


        #endregion

        #region API

        public static void OpenWorkingDir()
        {
#if UNITY_EDITOR_OSX
            EditorUtility.RevealInFinder(WorkingDir);
#else
            Application.OpenURL($"file://{WorkingDir}");   
#endif
        }


        /// <summary>
        /// 导出 CSV 文件
        /// </summary>
        /// <param name="sheetId"></param>
        /// <param name="tableName"></param>
        public static void ExportCSV()
        {
            //--------- 导出csv ---------------
            var csv = ExportGuruCSVString();
            // 执行备份和排序
            var finalCsv= BackupAndSortCSV(csv);
            // 写入文件
            File.WriteAllText(LocalizationFilePath, finalCsv);
        }
        
        /// <summary>
        /// 生成语言Keys
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static List<string> GenTermsKeysFromPath(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            
            List<string> list = new List<string>();
            var lines = LocalizationReader.ReadCSV(File.ReadAllText(filePath)); // 读取I2 格式的CSV
            
            string[] l;
            int i = 2;
            while (i < lines.Count)
            {
                l = lines[i];
                if (l != null)
                {
                    list.Add(l[0]);
                }
                i++;
            }
            return list;
        }

        /// <summary>
        /// 整理I2CSV, 老的在后面, 新的在前面
        /// </summary>
        /// <param name="csv"></param>
        /// <param name="toPath"></param>
        /// <returns></returns>
        private static string BackupAndSortCSV(string csv, string sperator = ",", string lineBr = "\n")
        {
            int i = 0;
            if (!File.Exists(BackupFilePath))
            {
                // 如果备份文件不存在, 则直接保留备份文件, 采用原文
                BackupFromData(csv);
                return csv; 
            }
            
            // ----------- 整理顺序 --------------
            List<string> prevKeys = GenTermsKeysFromPath(BackupFilePath); // 读取备份Keys

            string key;
            List<string[]> temp = new List<string[]>();            
            var lines = LocalizationReader.ReadCSV(csv);
            
            // #1.  记录本次新增的Terms
            i = 2;
            while ( i < lines.Count)
            {
                key = lines[i][0];
                if(!prevKeys.Contains(key)) temp.Add(lines[i]); 
                i++;
            }
            
            // #2. 后移所有新翻译
            List<string[]> newLines = new List<string[]>(lines.Count);
            i = 0;
            while (i < lines.Count)
            {
                if (!temp.Contains(lines[i]))
                {
                    newLines.Add(lines[i]);
                }
                i++;
            }
            if(temp.Count > 0) newLines.AddRange(temp);
            
            // #3. 组装并输出
            return BuildCSV(newLines, sperator, lineBr);
        }

        /// <summary>
        /// 构建字符串
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private static string BuildCSV(List<string[]> lines, string sperator = ",", string lineBr = "\n")
        {
            int i = 0;
            int j = 0;
            string  e = "";
            StringBuilder sb = new StringBuilder();
            while (i < lines.Count)
            {
                j = 0;
                while (j < lines[i].Length)
                {
                    e = lines[i][j];

                    if (e.Contains("\n") || e.Contains(","))
                    {
                        e = "\"" + e + "\"";
                    }

                    sb.Append(e);

                    if (j < lines[i].Length - 1)
                    {
                        sb.Append(sperator);
                    }
                    else
                    {
                        sb.Append(lineBr);
                    }

                    j++;
                }
                i++;
            }
            // 输出最终的排序后的CSV
            return sb.ToString(); 
        }



        /// <summary>
        /// 导入CSV
        /// </summary>
        public static bool ImportCSV()
        {
            if (File.Exists(LocalizationFilePath))
            {
                var csv = File.ReadAllText(LocalizationFilePath);
                if (csv.Contains("\r"))
                {
                    csv = csv.Replace("\r", "\n");
                    // Debug.Log($"-- found \\r in file");
                }
                return ImportGuruCSVString(csv);  // 导入文件
            }

            return false;
        }

        public static string FindLanguageCodeByName(string langName)
        {
            RecI2Languages rec = RecI2Languages.Load();
            if (rec != null)
            {
                return rec.GetCode(langName);
            }
            return "";
        }

        /// <summary>
        /// 调用同步命令
        /// </summary>
        /// <param name="sheetId"></param>
        /// <param name="tableName"></param>
        /// <param name="proxyUrl"></param>
        /// <param name="callback"></param>
        /// <param name="logName"></param>
        public static void CallSyncCmd(string sheetId, string tableName, L10NAction action = L10NAction.sync, string proxyUrl = "",
            Action<bool> callback = null, string logName = "")
        {
            if(string.IsNullOrEmpty(logName)) logName = DEFAULT_LOG_NAME;
            // --------- 检查环境 ---------
            CheckEnvironment();
            CheckMainLocalizationFile();
            // --------- 设置动作 ---------
            string actionName = action.ToString();
            string successToken = "Data sync successfully";
            if (action == L10NAction.translate)
            {
                successToken = "\"code\": 0,";
            }
            Debug.Log($"--- call cmd: {action}");
            //--------- 构建参数 ---------
            BuildParams(sheetId, tableName, actionName, LOCALIZATION_FILENAME, proxyUrl, logName);
            //--------- 调用命令 ---------
#if UNITY_EDITOR_OSX
            RunMacCommand(WorkingDir);
#else
            RunWinBatch(WorkingDir);    
#endif
            // --------- 等待命令执行完毕 ---------
            WaitCmdRunning(WorkingDir, logName, callback, successToken); // 等待命令支撑
        }

        /// <summary>
        /// 命令结束
        /// </summary>
        /// <param name="workingDir"></param>
        /// <param name="logName"></param>
        /// <param name="callback"></param>
        private static async void WaitCmdRunning(string workingDir, string logName, Action<bool> callback = null, string successToken = "")
        {
            string title = "执行同步命令";
            string info = "数据同步中...";

            ShowProgress(title, info, 0);
            
            string end_token = "l10n sync over";
            // string success_token = "Data sync successfully";
            
            bool isOver = false;
            bool isSuccess = false;
            
            string logPath = $"{workingDir}/{logName}";
            string[] lines;
            int header = 0;
            int i = 0, idx = 0;
            int catchLens = 30;
            int timeout = 50; // 超时50秒
            int runningTime = 0;
            int loadTimeout = 0;
            string l;

            //------- 执行 Editor 轮询等待 --------
            try
            {
                await Task.Delay(500); // 读取间隔 0.5s
                
                while (!File.Exists(logPath))
                {
                    await Task.Delay(2000); // 读取间隔 2s
                    loadTimeout++;

                    if (loadTimeout >= 60)
                    {
                        Debug.LogError($"[L10n] loading is timeout: {loadTimeout}");
                        callback?.Invoke(false);
                        return;
                    }
                }
                
                // Debug.Log($"FileExsits: {logPath}");
                
                while (!isOver && runningTime <= timeout)
                {
                    await Task.Delay(1000); // 读取间隔 1s
                    lines = File.ReadAllLines(logPath);
                    runningTime++;
                    ShowProgress(title, info, (float)runningTime/timeout);
                
                    if (lines.Length > 1)
                    {
                        header = lines.Length - 1;
                        idx = header;
                        i = 0;
                        while (i < catchLens && idx < lines.Length  && idx >= 0)
                        {
                            l = lines[idx];  // 从后往前读

                            if (l.Contains(end_token))
                            {
                                isOver = true;
                            }
                            if (l.Contains(successToken))
                            {
                                isSuccess = true;
                                isOver = true;
                                break;
                            }
                            i++;
                            idx = header - i;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
            EditorUtility.ClearProgressBar();
            callback?.Invoke(isSuccess);
        }
        
        /// <summary>
        /// 显示进度
        /// </summary>
        /// <param name="title"></param>
        /// <param name="info"></param>
        /// <param name="progress"></param>
        private static void ShowProgress(string title, string info, float progress)
        {
           if (!_showProgress)
           {
               EditorUtility.ClearProgressBar();
               return;
           }

           if (EditorUtility.DisplayCancelableProgressBar(title, info, progress))
           {
               EditorUtility.ClearProgressBar();
           }
        }

        // Run Mac command
        private static void RunMacCommand(string workingDir, string cmd = "")
        {
            if (string.IsNullOrEmpty(cmd)) cmd = SYNC_COMMAND;
            CmdRunner.OpenMacCommand(cmd, workingDir);
        }

        // Run Win bat
        private static void RunWinBatch(string workingDir, string cmd = "")
        {
            if (string.IsNullOrEmpty(cmd)) cmd = SYNC_BAT;
            CmdRunner.CallWinBat(cmd, workingDir);
        }


        #endregion

        #region 语言支持
        
        /// <summary>
        /// 检查语言支持
        /// </summary>
        /// <returns></returns>
        public static List<string> CheckUnsupported()
        {
            List<string> unsupported = new List<string>();
            var spt = SupportedLanguages.Load();
            if (spt != null)
            {
                var source = GetSourceData();
                foreach (var lan in source.mLanguages)
                {
                    if (!spt.HasCode(lan.Code))
                    {
                        unsupported.Add(lan.Code);
                    }
                }
                return unsupported;
            }
            return null;
        }

        
        /// <summary>
        /// 设置推荐语种
        /// </summary>
        /// <param name="allCount"></param>
        /// <param name="missingCount"></param>
        /// <param name="asset"></param>
        public static void SetupRecommendedLanguages(out int allCount, out int missingCount, out Object obj)
        {
            allCount = missingCount = 0;
            var asset = Resources.Load<LanguageSourceAsset>(LocalizationManager.GlobalSources[0]);
            obj = asset as Object;
            
           if (asset == null)
           {
                Debug.LogError($"--- Load Source Error ---");
                return;
           }
           
           Debug.Log($"--- <color=white>原始语言数量: {asset.mSource.mLanguages.Count}</color>");
           
           var recs  = RecI2Languages.Load();
           Debug.Log($"--- <color=yellow>导入语言数量: {recs.defaults.Length}</color>");
           
           List<DefLanguage> missing = new List<DefLanguage>();
           List<DefLanguage> existed = new List<DefLanguage>();
           List<string> deleted = new List<string>();
           List<string> removeKeys = new List<string>()
           {
                "fil", "no", "mr", "",
           };

           string code = "";
           //统计要删除的语言
           for (int i = 0; i < asset.mSource.mLanguages.Count; i++)
           {
               code = asset.mSource.mLanguages[i].Code;
               if (string.IsNullOrEmpty(code) || removeKeys.Contains(code))
               {
                   deleted.Add(asset.mSource.mLanguages[i].Name);
               }
           }

            // 统计要修改的语言和缺失的语言       
           DefLanguage def = null;
           for (int i = 0; i < recs.defaults.Length; i++)
           {
                def = recs.defaults[i];
                if (def != null)
                {
                     var l = asset.mSource.mLanguages.FirstOrDefault(c => c.Code == def.code);

                     // ------------  修复部分Code --------------- 
                     if (l == null)
                     {
                          string scode = "";
                          string replaced = "";
                          switch (def.code)
                          {
                               case "es-ES": 
                                    scode = "es";
                                    replaced = "es-ES";
                                    break;
                               case "eu-ES": 
                                   scode = "eu";
                                   replaced = "eu-ES";
                                   break;
                               case "gl-ES": 
                                   scode = "gl";
                                   replaced = "gl-ES";
                                   break;
                               case "fr-FR": 
                                    scode = "fr";
                                    replaced = "fr-FR";
                                    break;
                               case "pt-PT": 
                                    scode = "pt";
                                    replaced = "pt-PT";
                                    break;
                               case "zh-CN":
                                    scode = "zh";
                                    replaced = "zh-CN";
                                    break;
                               case "de-DE":
                                    scode = "de";
                                    replaced = "de-DE";
                                    break;
                               case "sv-SE":
                                    scode = "sv";
                                    replaced = "sv-SE";
                                    break;
                               case "it-IT":
                                    scode = "it";
                                    replaced = "it-IT";
                                    break;
                               case "nl-NL":
                                    scode = "nl";
                                    replaced = "nl-NL";
                                    break;
                               case "no-NO":
                                   scode = "no";
                                   replaced = "nb-NO";
                                   break;
                          }
                          
                          if (!string.IsNullOrEmpty(scode))
                          {
                               l = asset.mSource.mLanguages.FirstOrDefault(c => c.Code == scode);

                               if (l != null)
                               {
                                    existed.Add(def);
                                    l.Code = replaced;
                               }
                          }
                     }

                     
                     if (l == null)
                     {
                          Debug.Log($"+ <color=#88ff00>find missing code({def.id}): [{def.code}] - {def.name}</color>");
                          missing.Add(def);
                     }
                     else
                     {
                          existed.Add(def);
                     }
                }
           }

           //添加缺失的语言对象
           if (missing.Count > 0)
           {
               Debug.Log($"--- <color=yellow>缺失语言数量: {missing.Count}</color>");
               for (int i = 0; i < missing.Count; i++)
               {
                   asset.mSource.AddLanguage(missing[i].name, missing[i].code);
               }
           }

           if (deleted.Count > 0)
           {
               Debug.Log($"--- <color=orange>移除语言数量: {deleted.Count}</color>");
               for (int i = 0; i < deleted.Count; i++)
               {
                   asset.mSource.RemoveLanguage(deleted[i]);
               }
           }

           missingCount = missing.Count;
           allCount = asset.mSource.mLanguages.Count;

           ResortLanguages(asset.mSource);
           
           EditorUtility.SetDirty(asset);
           AssetDatabase.SaveAssets();
           AssetDatabase.Refresh();
           Selection.activeObject = null;
           Debug.Log($"---- Save I2 Language Source ----");
        }
        
        /// <summary>
        /// 重排整个语言队列
        /// </summary>
        /// <param name="source"></param>
        public static void ResortLanguages()
        {
            var asset = LoadI2Asset();
            LanguageSourceData source = asset.mSource;
            ResortLanguages(source);
            source.Editor_SetDirty();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
        }
        
        public static void ResortLanguages(LanguageSourceData source)
        {
            var langs = source.mLanguages;

            var origin_codes = new List<string>(source.mLanguages.Count);
            for (int i = 0;  i < langs.Count; i++)
            {
                origin_codes.Add(langs[i].Code);
            }

            var en = langs.FirstOrDefault(c => c.Code == "en");
            if (en != null) langs.Remove(en);
            // --- Sort Languages ---
            langs.Sort((a, b) => CompareLanguageCode(a.Code, b.Code));
            
            if(en != null) langs.Insert(0, en);
            source.mLanguages = langs;
            
            var terms = source.mTerms;
            List<TermData> termTmp = new List<TermData>();
            string[] lns;
            string[] lnTmp;
            bool excError = false;
            for (int i = 0; i < terms.Count; i++)
            {
                lns = terms[i].Languages;
                lnTmp = new string[langs.Count];
                for (int j = 0; j < lns.Length; j++)
                {
                    int idx = origin_codes.IndexOf(langs[j].Code);
                    if (idx >= 0 && idx < lns.Length)
                    {
                        lnTmp[j] = lns[idx];
                    }
                    else
                    {
                        excError = true;
                        Debug.LogError($"--- Error on Swape items {i} -> {idx}");
                        return;
                    }
                }

                if (!excError) terms[i].Languages = lnTmp;
            }
            source.mTerms = terms;
            source.Editor_SetDirty();
        }
        
        /// <summary>
        /// 对比语言Code
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static int CompareLanguageCode(string a, string b)
        {
            // 按照 Char 的大小进行排序
            if (a[0] > b[0]) return 1;
            if (a[0] < b[0]) return -1;

            if (a.Length > 1 && b.Length > 1)
            {
                if (a[1] > b[1]) return 1;
                if (a[1] < b[1]) return -1;
                
                if (a.Length > 2 && b.Length > 2)
                {
                    if (a[2] > b[2]) return 1;
                    if (a[2] < b[2]) return -1;
                
                    if (a.Length > 3 && b.Length > 3)
                    {
                        if (a[3] > b[3]) return 1;
                        if (a[3] < b[3]) return -1;
                    }
                }
            }
            
            return 0;
        }
        
        /// <summary>
        /// 清除所有翻译
        /// </summary>
        /// <param name="source"></param>
        public static void CleanAllTerms(LanguageSourceData source)
        {
            var terms = source.mTerms;
            for (int i = 0; i < terms.Count; i++)
            {
                for (int j = 1; j < terms[i].Languages.Length; j++)
                {
                    terms[i].Languages[j] = "";
                }
            }
            source.Editor_SetDirty();
        }

        #endregion
        
        #region 测试接口
    
        // [MenuItem("Guru/I2 Localization/导出 [CSV]")]
        [Test]
        public static void TestExportToFile()
        {
            var csv = ExportGuruCSVString();
            if (string.IsNullOrEmpty(csv))
            {
                EditorUtility.DisplayDialog("导出Guru多语言配置", "I2 Guru定制CSV文件导出失败...", "OK");
                return;
            }

            var final = BackupAndSortCSV(csv);
            string path = LocalizationFilePath;
            File.WriteAllText(path, final);
#if UNITY_EDITOR_OSX
            EditorUtility.RevealInFinder(path);
#endif
        }
        
        // [MenuItem("Guru/I2 Localization/导入 [CSV]")]
        [Test]
        public static void TestImportFromFile()
        {
            string path = LocalizationFilePath;
            if (!File.Exists(path))
            {
                path = EditorUtility.OpenFilePanelWithFilters("导入Guru多语言配置", $"{Application.dataPath}/../",
                    new string[] { "csv,txt" });
            }

            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("导入Guru多语言配置", $"{path}\n选定文件不存在", "OK");
                return;
            }

            
            bool res = ImportGuruCsvFromFile(path);
            if (res)
            {
                EditorUtility.DisplayDialog("导入Guru多语言配置", $"配置导入成功", "OK");
                if (null != _asset) Selection.activeObject = _asset;
            }
            else
            {
                
                EditorUtility.DisplayDialog("导入Guru多语言配置", $"配置导入失败", "OK");
            }
        }


        [Test]
        public static void Test_FindMissingCode()
        {
            List<string> codes = new List<string>
                {"af","am","ar","az","be","bg","bn","ca","cs","da","de-DE","el","es-ES","es-US","et","eu-ES","fa","fi","fr-CA","fr-FR","gl-ES","gu","he","hi","hr","hu","hy","id","is","it-IT","ja","ka","kk","km","kn","ko","ky","lo","lt","lv","mk","ml","mn","mr","ms","ms-MY","my","nb","ne","nl-NL","pa","pl","pt-BR","pt-PT","ro","ru","si","sk","sl","sq","sr","sv-SE","sw","ta","te","th","tl","tr","uk","ur","vi","zh-CN","zh-HK","zh-TW","zu"};

            Debug.Log($">>> input codes: {codes.Count}");
            
            var doc = RecI2Languages.Load();

            Debug.Log($">>> default codes: {doc.defaults.Length}");

            if (codes.Count == doc.defaults.Length)
            {
                Debug.Log($">>>> All Matched!");
                return;
            }

            foreach (var d  in doc.defaults)
            {
                if (codes.IndexOf(d.code) < 0)
                {
                    Debug.LogError($"=== Fount missing lang: {d.name} [{d.code}]");
                }
            }

        }

        #endregion
        
    }
    
}
