using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Guru.Vibration
{
    public class VibrationService
    {
#if UNITY_EDITOR
        private static readonly IVibrationLib _vibrationLib = new VibrationLibFake();
#elif UNITY_ANDROID
        private static readonly IVibrationLib _vibrationLib = new VibrationLibAndroid();
#elif UNITY_IOS
        private static readonly IVibrationLib _vibrationLib = new VibrationLibIOS();
#endif

        /// <summary>
        /// 设备是否支持震动功能
        /// </summary>
        public static bool HasVibrator()
        {
            return _vibrationLib.HasVibrator();
        }

        /// <summary>
        /// 设备是否支持振动强度控制
        /// </summary>
        public static bool HasAmplitudeControl()
        {
            return _vibrationLib.HasAmplitudeControl();
        }

        /// <summary>
        /// 设备是否支持预设效果 (Light, Medium, Heavy, Selection, Double)
        /// 判断条件需要同时满足以下条件 ：
        /// 1. 高通芯片；
        /// 2. 不是ViVo的手机；
        /// 3. 不是低内存的手机;
        /// 4. 支持EFFECT_TICK, EFFECT_CLICK, EFFECT_HEAVY_CLICK效果的手机
        /// if (VibrationService.HasVibrationEffect())
        /// {
        ///     VibrationService.Light();
        /// }
        /// </summary>
        public static bool HasVibrationEffect()
        {
            return _vibrationLib.HasVibrationEffect();
        }

        /// <summary>
        /// 设备是否支持自定义震动模式
        /// if (VibrationService.HasCustomVibrationsSupport())
        /// {
        ///     VibrationService.CustomVibrate(new VibrateData(
        ///         pattern: new List<uint> {0, 203, 0, 200, 0, 252, 0, 150},
        ///         intensities: new List<byte> {0, 75, 61, 79, 57, 75, 57, 97}));
        /// }
        /// </summary>
        public static bool HasCustomVibrationsSupport()
        {
            return _vibrationLib.HasCustomVibrationsSupport();
        }

        #region 中台预设效果

        public static void Light()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style: VibrationStyle.Light
            ));
        }

        public static void Medium()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style: VibrationStyle.Medium
            ));
        }

        public static void Heavy()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style: VibrationStyle.Heavy
            ));
        }

        public static void Selection()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style: VibrationStyle.Selection
            ));
        }

        public static void Double()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style: VibrationStyle.Double
            ));
        }

        #endregion

        public static void Success()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style: VibrationStyle.Success
            ));
        }

        public static void Warning()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style: VibrationStyle.Warning
            ));
        }

        public static void Error()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style: VibrationStyle.Error
            ));
        }

        /// <summary>
        /// 取消震动， 注意 ： 并不是一种效果，而是取消当前正在进行的震动
        /// </summary>
        public static void CancelVibrate()
        {
            _vibrationLib.Vibrate(VibrationCommand.Cancel);
        }

        /// <summary>
        /// 自定义震动效果，注意： 自定义方式由传入的 VibrateData 决定，具体效果可在DebugView中体验
        /// </summary>
        /// <param name="data"></param>
        public static void CustomVibrate(VibrateData data)
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, data);
        }
    }
}