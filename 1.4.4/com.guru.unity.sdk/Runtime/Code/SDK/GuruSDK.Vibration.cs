
namespace Guru
{
    using System.Collections.Generic;
    using Guru.Vibration;
    
    // 预定义的震动类型
    public enum VibrateType
    {
        Light,
        Medium,
        Heavy,
        Double,
        Selection,
            
        // Success,
        // Warning,
        // Error,
        // CancelVibrate
    }
    
    /// <summary>
    /// 震动部分接口
    /// </summary>
    public partial class GuruSDK
    {
        private const string TAG_VIBRATION = "[Vibration]";
        /// <summary>
        /// 最大震动强度
        /// </summary> 
        private const float MAX_VIBRATION_INTENSITY = 255;

        /// <summary>
        /// 最小震动强度
        /// </summary>
        private const float MIN_VIBRATION_INTENSITY = 1;
        
        /// <summary>
        /// 最小震动时长(毫秒)
        /// </summary>
        private const int MIN_VIBRATION_DURATION = 1;


        
        /// <summary>
        /// 检查震动功能是否可用
        /// </summary>
        public static bool HasVibrationCapability()
        {
            return VibrationService.HasVibrator();
        }

        /// <summary>
        /// 判断设备是否支持高级震动特效
        /// </summary> 
        public static bool SupportsVibrationEffect()
        {
            return VibrationService.HasVibrationEffect();
        }

        /// <summary>
        /// 判断设备是否支持震动强度调节
        /// </summary>
        public static bool SupportsAmplitudeControl()
        {
            return VibrationService.HasAmplitudeControl();
        }

        /// <summary>
        /// 判断设备是否支持自定义震动参数
        /// </summary>
        public static bool SupportsCustomVibration()
        {
            return VibrationService.HasCustomVibrationsSupport();
        }

        /// <summary>
        /// 执行预设震动效果
        /// </summary>
        /// <param name="vibrateType">震动类型</param>
        /// <returns>是否执行成功</returns>
        public static bool Vibrate(VibrateType vibrateType)
        {
            try
            {
                if (!HasVibrationCapability())
                {
                    UnityEngine.Debug.LogWarning($"{TAG_VIBRATION} Device does not support vibration!");
                    return false;
                }

                switch (vibrateType)
                {
                    case VibrateType.Light: VibrationService.Light(); break;
                    case VibrateType.Medium: VibrationService.Medium(); break;
                    case VibrateType.Heavy: VibrationService.Heavy(); break;
                    case VibrateType.Double: VibrationService.Double(); break;
                    case VibrateType.Selection: VibrationService.Selection(); break;
                }
                
                UnityEngine.Debug.Log($"{TAG_VIBRATION} Triggered vibration type: {vibrateType}");
                return true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"{TAG_VIBRATION} Failed to execute vibration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 执行自定义震动效果
        /// 目前只对 Android 有效
        /// </summary>
        /// <param name="pattern">震动时长模式数组(毫秒)</param>
        /// <param name="intensities">震动强度数组(1-255)</param>
        /// <returns>是否执行成功</returns>
        public static bool CustomVibrate(List<int> pattern, List<int> intensities)
        {
            // 是否支持震动
            if (!SupportsCustomVibration())
            {
                UnityEngine.Debug.LogError($"{TAG_VIBRATION} Device does not support custom vibration!");
                return false;
            }

            // 参数是否正确
            if (!ValidateVibrationParams(pattern, intensities))
            {
                return false;
            }
            
            VibrationService.CustomVibrate(new VibrateData()
            {
                pattern = pattern,
                intensities = intensities,
            });
            
            UnityEngine.Debug.Log($"{TAG_VIBRATION} Triggered custom vibration with {pattern.Count} patterns");
            return true;
        }

        /// <summary>
        /// 验证震动参数是否有效
        /// </summary>
        private static bool ValidateVibrationParams(List<int> pattern, List<int> intensities)
        {
            if (pattern == null || intensities == null)
            {
                UnityEngine.Debug.LogError($"{TAG_VIBRATION} Invalid parameters: pattern or intensities is null!");
                return false;
            }

            if (pattern.Count != intensities.Count)
            {
                UnityEngine.Debug.LogError($"{TAG_VIBRATION} Pattern and intensities must have same length!");
                return false;
            }

            // 验证震动强度
            foreach (float intensity in intensities)
            {
                if (intensity < MIN_VIBRATION_INTENSITY || intensity > MAX_VIBRATION_INTENSITY)
                {
                    UnityEngine.Debug.LogError($"{TAG_VIBRATION} Invalid intensity: {intensity}. Must be between {MIN_VIBRATION_INTENSITY} and {MAX_VIBRATION_INTENSITY}");
                    return false;
                }
            }

            return true;
        }
    }
}