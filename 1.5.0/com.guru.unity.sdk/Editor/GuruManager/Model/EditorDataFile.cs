namespace Guru.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    
    public class EaseConfigFile
    {
        private Dictionary<string, string> _dataDict;
        private string _filePath;

        protected bool ReadFile(string path)
        {
            _filePath = path;
            
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                int len = lines.Length;
                _dataDict = new Dictionary<string, string>(len);

                string key = "";
                string value = "";
                
                for (int i=0; i< len; i++)
                {
                    var line = lines[i];
                    if (line.Contains("="))
                    {
                        key = "";
                        value = "";
                        var kv = line.Split('=');
                        if(kv.Length > 0) key = kv[0];
                        if(kv.Length > 1) value = kv[1];
                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        {
                            _dataDict[key] = value;
                        }
                    }
                }
                return true;
            }
            
            _dataDict = new Dictionary<string, string>(10);
            var dir = Directory.GetParent(path);
            if(dir is { Exists: false }) dir.Create();
            return false;
        }

        public void Save()
        {
            if (_dataDict == null || _dataDict.Count < 1) return;

            List<string> lines = new List<string>(_dataDict.Count);
            foreach (var key in _dataDict.Keys)
            {
                lines.Add($"{key}={_dataDict[key].ToString()}");
            }
            
            if(!string.IsNullOrEmpty(_filePath))
                File.WriteAllLines(_filePath, lines);
        }
        
        public void Set(string key, object value)
        {
            if (_dataDict == null) _dataDict = new Dictionary<string, string>(10);
            _dataDict[key] = value.ToString();
            Save();
        }

        public string Get(string key) => _dataDict.ContainsKey(key) ? _dataDict[key] : "";

        public bool TryGet(string key, out string value)
        {
            value = "";
            return _dataDict?.TryGetValue(key, out value) ?? false;
        }

        public bool GetBool(string key, bool defaultVal = false)
        {
            if (TryGet(key, out var str))
            {
                return (str.ToLower() == "true" || str == "1");
            }
            return defaultVal;
        }
        
        
        public int GetInt(string key, int defaultVal = 0)
        {
            if (TryGet(key, out var str))
            {
                var inVal = 0;
                if (int.TryParse(str, out inVal))
                {
                    return inVal;
                }
            }
            return defaultVal;
        }
        
        public float GetFloat(string key, float defaultVal = 0)
        {
            if (TryGet(key, out var str))
            {
                float val = 0;
                if (float.TryParse(str, out val))
                {
                    return val;
                }
            }
            return defaultVal;
        }
        
    }
}