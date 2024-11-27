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

        public static bool HasHasVibrator()
        {
            return _vibrationLib.HasVibrator();
        }

        public static bool HasAmplitudeControl()
        {
            return _vibrationLib.HasAmplitudeControl();
        }

        public static bool HasCustomVibrationsSupport()
        {
            return _vibrationLib.HasCustomVibrationsSupport();
        }

        public static bool HasVibrationEffect()
        {
            return _vibrationLib.HasVibrationEffect();
        }

        public static void Success()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style : VibrationStyle.Success
            ));
        }

        public static void Warning()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style : VibrationStyle.Warning
            ));
        }

        public static void Error()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style : VibrationStyle.Error
            ));
        }

        public static void Light()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style : VibrationStyle.Light
            ));
        }

        public static void Medium()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style : VibrationStyle.Medium
            ));
        }

        public static void Heavy()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style : VibrationStyle.Heavy
            ));
        }

        public static void Selection()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style : VibrationStyle.Selection
            ));
        }

        public static void Double()
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, new VibrateData
            (
                style : VibrationStyle.Double
            ));
        }

        public static void CancelVibrate()
        {
            _vibrationLib.Vibrate(VibrationCommand.Cancel);
        }

        public static void CustomVibrate(VibrateData data)
        {
            _vibrationLib.Vibrate(VibrationCommand.Vibrate, data);
        }
    }
}