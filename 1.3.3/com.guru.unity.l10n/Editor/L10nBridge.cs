


namespace Guru
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using I2.Loc;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using Newtonsoft.Json;
    using API = GuruI2LocAPI;
    
    [Serializable]
    public class LanguageInfo
    {
        public string code;
        public string family;
        public string name;
        public string nameCN;

        public void Parse(string raw)
        {
            raw = raw.Replace(": [", ":[")
                .Replace(", ", ",")
                .Replace("],", "")
                .Replace("\"", "")
                .Replace("'", "");
            int idx = raw.IndexOf(":[");

            code = raw.Substring(0, idx);
            var arr = raw.Substring(idx+2).Split(',');

            if (arr.Length > 0) family = arr[0];
            if (arr.Length > 1) name = arr[1];
            if (arr.Length > 2) nameCN = arr[2];
        }

        /// <summary>
        /// 转化为字典变量
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"code\":\"{code}\",\"data\":[\"{family}\",\"{name}\",\"{nameCN}\"]");
            sb.Append("}");
            return sb.ToString();
        }
            
        public static LanguageInfo ByPyLine(string py_line)
        {
            var data = new LanguageInfo();
            data.Parse(py_line);
            return data;
        }

    }
    
    [Serializable]
    public class SupportedLanguages
    {
        public static string FilePath => $"{API.CmdDir}/supported.json";
        
        /// <summary>
        /// 支持的所有语言代码
        /// </summary>
        public string[] codes;
        
        /// <summary>
        /// 支持语言的原始信息
        /// </summary>
        public LanguageInfo[] infos;

        /// <summary>
        /// 加载文件
        /// </summary>
        /// <returns></returns>
        public static SupportedLanguages Load()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                try
                {
                    return JsonConvert.DeserializeObject<SupportedLanguages>(json);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return null;
        }

        /// <summary>
        /// 是否包含语言Code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool HasCode(string code) => codes.Contains(code);
    }


    [Serializable]
    public class I2LanguageDef
    {
        public string title;
        public string code;
        public string googleCode;
    }

    [Serializable]
    public class I2Languages
    {
        public static string FilePath =>
            Path.GetFullPath($"{API.CmdDir}/i2_languages.json");

        public string version;
        public List<I2LanguageDef> defines;
        public List<string> missings;


        public static I2Languages Load()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                try
                {
                    return JsonConvert.DeserializeObject<I2Languages>(json);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return null;
        }
        
        public void Save() => File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented));

    }


    /// <summary>
    /// 默认语言配置 (来自中台)
    /// </summary>
    [Serializable]
    public class DefLanguage
    {
        public int id;
        public string name;
        public string code;
        public string desc;
    }


    /// <summary>
    /// 默认支持语言
    /// </summary>
    [Serializable]
    public class RecI2Languages
    {
        /// <summary>
        /// 预加载文件
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static RecI2Languages Load()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                try
                {
                    return JsonConvert.DeserializeObject<RecI2Languages>(json);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                Debug.LogError($"File not exsited: {FilePath}");
            }
            return null;
        }
        
        
        public static readonly string FilePath = $"{API.CmdDir}/recommends.json";
        
        public DefLanguage[] defaults;

        public void Save()
        {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public DefLanguage Get(string code)
        {
            return defaults.FirstOrDefault(c => c.code == code);
        }

        public string GetCode(string name)
        {
            var l = defaults.FirstOrDefault(c => c.name == name);
            return l?.code ?? "";
        }

    }


    /// <summary>
    /// L10N 工具桥接和工具接口
    /// </summary>
    public class L10nBridge
    {
        public static bool CreatL10nSupported()
        {
            // 获取home路径, MacOS有效
            var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = $"{home}/.guru/guru_config/l10n/l10n_engine.py";
            if (File.Exists(path))
            {
                
                List<LanguageInfo> items = new List<LanguageInfo>();
                List<string> codes = new List<string>();
                
                var lines = File.ReadAllLines(path);
                int startIdx = 10000;
                string l = "";
                for(int i = 0; i < lines.Length; i ++)
                {
                    l = lines[i].TrimStart();
                    if (l.Contains("l10n_unity_map = {"))
                    {
                        startIdx = i + 1;
                        continue;
                    }

                    if (i >= startIdx)
                    {
                        if (l.StartsWith("}")) break;
                        var item = LanguageInfo.ByPyLine(l);
                        items.Add(item);
                        codes.Add(item.code.Replace("_", "-"));
                    }
                }

                // 支持文档
                SupportedLanguages spt = new SupportedLanguages()
                {
                    codes =  codes.ToArray(),
                    infos = items.ToArray(),
                };
                File.WriteAllText(SupportedLanguages.FilePath, JsonConvert.SerializeObject(spt));
                
                EditorUtility.RevealInFinder(Directory.GetParent(SupportedLanguages.FilePath)?.FullName);

            }


            return false;
        }

        /// <summary>
        /// 导出I2所有支持的事件
        /// </summary>
        private static void ExportAllI2Languages()
        {
            var supported = SupportedLanguages.Load();
            
            var path = I2Languages.FilePath;
            var dict = I2.Loc.GoogleLanguages.mLanguageDef;

            List<string> missing = new List<string>(50);
            List<I2LanguageDef> list = new List<I2LanguageDef>();  
            GoogleLanguages.LanguageCodeDef source;

            string _code;
            foreach (var key in dict.Keys)
            {
                source = dict[key];
                _code = source.Code;
                list.Add(new I2LanguageDef()
                {
                    title =  key,
                    code = _code,
                    googleCode = source.GoogleCode,
                });

                if (!supported.HasCode(_code))
                {
                    missing.Add(_code.Replace("-", "_"));
                }
            }

            I2Languages languages = new I2Languages()
            {
                version = LocalizationManager.GetVersion(),
                defines =  list,
                missings =  missing,
            };
            languages.Save();
            
            EditorUtility.RevealInFinder(Directory.GetParent(path)?.FullName);
        }


        private static bool PickupFitableLanguage = true;
        
        /// <summary>
        /// 导出适配的语言
        /// ----- #1 先导出这个文件
        /// </summary>
        private static void ExportRecommendLanguages()
        {
            var def_path = $"{API.FilesDir}/lang_codes.txt";
            var deflines = File.ReadAllLines(def_path);

            List<DefLanguage> defLanguages = new List<DefLanguage>();
            List<string> missing = new List<string>(50);
            // I2支持的语言
            var dict = I2.Loc.GoogleLanguages.mLanguageDef;
            List<I2LanguageDef> list = new List<I2LanguageDef>();  
            GoogleLanguages.LanguageCodeDef source;
            
            string _code;
            foreach (var key in dict.Keys)
            {
                source = dict[key];
                _code = source.Code;
                list.Add(new I2LanguageDef()
                {
                    title =  key,
                    code = _code,
                    googleCode = source.GoogleCode,
                });
            }
            
            
            //----------- 对照标准化语言, 生成对应的语言标签 -----------------
            string code, name, desc;
            string[] raw;
            for (int i = 0; i < deflines.Length; i++)
            {
                if(deflines[i].Contains('#')) continue; // 略过注释
                
                //Afrikaans|南非语|af
                raw = deflines[i].Replace("_", "-").Split('|');
                name = raw[0];
                desc = raw[1];
                code = raw[2];

                I2LanguageDef d = list.FirstOrDefault(c => c.code == code);

                if (PickupFitableLanguage)
                {
                    // 1. 匹配Code (-)
                    if (d == null)
                    {
                        d = list.FirstOrDefault(c =>
                        {
                            if (code.Contains('-') && c.code == code.Split('-')[0])
                            {
                                code = c.code;
                                return true;
                            }
                            return false;
                        });
                    }
                    
                    // 2. 匹配 Name
                    if (d == null)
                    {
                        d = list.FirstOrDefault(c =>
                        {
                            if (c.title.Contains(name))
                            {
                                code = c.code;
                                return true;
                            }
                            return false;
                        });
                    }
                }


                if (d == null)
                {
                    missing.Add($"{code}|{name}");
                }
                else
                {
                    defLanguages.Add(new DefLanguage()
                    {
                        id = i + 1,
                        name = name,
                        code = code,
                        desc = desc,
                    });
                }
            }


            //保存文件
            I2Languages languages = new I2Languages()
            {
                version = LocalizationManager.GetVersion(),
                defines =  list,
                missings =  missing,
            };
            languages.Save();


            RecI2Languages recs = new RecI2Languages()
            {
                defaults = defLanguages.ToArray(),
            };
            recs.Save();

            Debug.Log($"All default languages ---> {recs.defaults.Length}");
            
            EditorUtility.RevealInFinder(Directory.GetParent(I2Languages.FilePath)?.FullName);
            
        }

        /// <summary>
        /// 重置所有的语言Code
        /// </summary>
        private static void ExportRecommendLanguagesV2()
        {
            var code_file_path = $"{API.FilesDir}/files/lang_codes.txt";

            string[] codes = File.ReadAllLines(code_file_path);

            List<DefLanguage> defLanguages = new List<DefLanguage>();
            List<string> missing = new List<string>(50);
            // I2支持的语言
            var dict = I2.Loc.GoogleLanguages.mLanguageDef;
            List<I2LanguageDef> list = new List<I2LanguageDef>();  
            GoogleLanguages.LanguageCodeDef source;
            
            
            string _code;
            foreach (var key in dict.Keys)
            {
                source = dict[key];
                _code = source.Code;
                list.Add(new I2LanguageDef()
                {
                    title =  key,
                    code = _code,
                    googleCode = source.GoogleCode,
                });
            }
            
            //----------- 对照标准化语言, 生成对应的语言标签 -----------------
            string code = "";
            for (int i = 0; i < codes.Length; i++)
            {
                if(codes[i].Contains('#')) continue; // 略过注释
                
                code = codes[i].Replace(" ", "");

                I2LanguageDef d = list.FirstOrDefault(c => c.code == code);

                if (PickupFitableLanguage)
                {
                    // 1. 匹配Code
                    if (d == null)
                    {
                        d = list.FirstOrDefault(c =>
                        {
                            if (code.Contains('-') && c.code == code.Split('-')[0])
                            {
                                code = c.code;
                                return true;
                            }
                            return false;
                        });
                    }
                }


                if (d == null)
                {
                    missing.Add($"{code}");
                }
                else
                {
                    defLanguages.Add(new DefLanguage()
                    {
                        id = i + 1,
                        name = "",
                        code = code,
                    });
                }
            }
            
            //保存文件
            I2Languages languages = new I2Languages()
            {
                version = LocalizationManager.GetVersion(),
                defines =  list,
                missings =  missing,
            };
            languages.Save();


            RecI2Languages recs = new RecI2Languages()
            {
                defaults = defLanguages.ToArray(),
            };
            recs.Save();

            Debug.Log($"All default languages ---> {recs.defaults.Length}");
            
            EditorUtility.RevealInFinder(Directory.GetParent(I2Languages.FilePath)?.FullName);
            
        }

        [Test]
        public static void Update_SupportLanguages()
        {
            // var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            // Debug.Log(home);
            CreatL10nSupported();
        }
        
        /// <summary>
        /// 生成所有I2当前版本支持的语言
        /// </summary>
        [Test]
        public static void Export__S1__Recommends()
        {
            ExportRecommendLanguages();
        }


    }
    
    
}