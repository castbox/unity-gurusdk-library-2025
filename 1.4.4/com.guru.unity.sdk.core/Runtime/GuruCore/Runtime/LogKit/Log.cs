

namespace Guru
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    
    public enum LogLevel
    {
        None = 0,       
        Exception = 1,  
        Error = 2,
        Warning = 3,
        Info = 4,
        Max = 5,
    }
    
    public static class Log
    {
        private static LogLevel _logLevel;
        private static bool _logFile = true;
        private static int _logFilePersistentTime = 3;  //日志文件保存3天
        private static string _logFileDir;
        private static StreamWriter _logFileWriter;
        
        public static LogLevel LogLevel { get => _logLevel; set => _logLevel = value; }
        public static bool LogFile { get => _logFile; set => _logFile = value; }
        
        static Log()
        {
            _logLevel = PlatformUtil.IsDebug() ? LogLevel.Max : LogLevel.Error;
            _logFileDir = string.Format("{0}/Log/", Application.persistentDataPath);
            
            //删除超出保存日期日志文件
            DeleteOutOfPersistentLogFile();
        }
        
        public static void I(object msg, params object[] args)
        {
            I(null, msg, args);
        }
        
        public static void I(string tag, object msg, params object[] args)
        {
            if (_logLevel < LogLevel.Info)
                return;

            string message = GetLogMessage(tag, msg, args);
            Debug.Log(message);
            if (_logFile)
                Log2File(message);
        }

        public static void W(object msg, params object[] args)
        {
            W(null, msg, args);
        }
        
        public static void W(string tag, object msg, params object[] args)
        {
            if (_logLevel < LogLevel.Warning)
                return;

            string message = GetLogMessage(tag, msg, args);
            Debug.LogWarning(message);
            if (_logFile)
                Log2File(message);
        }
        
        public static void E(object msg, params object[] args)
        {
            E(null, msg, args);
        }
        
        public static void E(string tag, object msg, params object[] args)
        {
            if (_logLevel < LogLevel.Error)
                return;

            string message = GetLogMessage(tag, msg, args);
            Debug.LogError(message);
            if (_logFile)
                Log2File(message);
        }
        
        public static void Exception(Exception e)
        {
            if (_logLevel < LogLevel.Exception)
                return;

            Debug.LogException(e);
            if (_logFile)
                Log2File(e.ToString());
        }

        private static string GetLogMessage(string tag, object msg, object[] args)
        {
            string message = null;
            if (args == null || args.Length == 0)
                message = msg.ToString();
            else
                message = string.Format(msg.ToString(), args);

            if (tag.IsNullOrEmpty())
                return message;
            else
                return string.Format("[{0}]::{1}", tag, message);
        }
        
        private static async Task Log2File(string message, bool error = false)
        {
            if (_logFileWriter == null)
            {
                string logFileName = DateTime.Now.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
                logFileName += ".log";
                string fullpath = _logFileDir + logFileName;
                try
                {
                    if (!Directory.Exists(_logFileDir))
                        Directory.CreateDirectory(_logFileDir);
                    _logFileWriter = File.AppendText(fullpath);
                    _logFileWriter.AutoFlush = true;
                }
                catch (Exception e)
                {
                    _logFileWriter = null;
                    Debug.LogError("LogToCache() " + e.Message + e.StackTrace);
                    return;
                }
            }

            if (_logFileWriter != null)
            {
                try
                {
                    await _logFileWriter.WriteLineAsync(message);
                    if (error)
                        await _logFileWriter.WriteLineAsync(StackTraceUtility.ExtractStackTrace());
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
        
        private static void DeleteOutOfPersistentLogFile()
        {
            try
            {
                if (!Directory.Exists(_logFileDir))
                    return;
                
                DateTime now = DateTime.Now;
                string[] files = Directory.GetFiles(_logFileDir);
                for (int i = 0; i < files.Length; i++)
                {
                    string fileName = Path.GetFileNameWithoutExtension(files[i]);
                    DateTime.TryParseExact(fileName, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fileTime);
                    if ((now - fileTime).Days >= _logFilePersistentTime)
                    {
                        File.Delete(files[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("删除过期日志文件失败");
            }
        }
    }
}