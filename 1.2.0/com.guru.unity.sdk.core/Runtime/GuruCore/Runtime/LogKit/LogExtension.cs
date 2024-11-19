namespace Guru
{
    public static class LogExtension
    {
        public static void Log(this object obj, string format, params object[] args)
        {
            Guru.Log.I(GetLogTag(obj), format, args);
        }
        
        public static void LogWarning(this object obj, string format, params object[] args)
        {
            Guru.Log.W(GetLogTag(obj), format, args);
        }

        public static void LogError(this object obj, string format, params object[] args)
        {
            Guru.Log.E(GetLogTag(obj), format, args);
        }

        private static string GetLogTag(object obj)
        {
            return obj.GetType().ToString();
        }
    }
}
