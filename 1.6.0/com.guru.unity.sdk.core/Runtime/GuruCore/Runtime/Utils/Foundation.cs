#nullable enable
using System;

namespace Guru
{
    public static class Foundation
    {
        public static void SafeRun(Action action, Action<Exception>? onError = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                Log.W($"[SafeRun] Error: {ex.Message}");
            }
        }
        
    }
    
    
    
    public static class StringExtensions
    {
        public static string ToSecretString(this string? value, int maxVisible = 2)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value != null && value.Length <= maxVisible * 2)
            {
                return new string('*', value.Length);
            }

            var visiblePart = value![..maxVisible];
            var hiddenPart = new string('*', value.Length - maxVisible * 2);
            var endPart = value[^maxVisible..];
            return $"{visiblePart}{hiddenPart}{endPart}";
        }
    }
}