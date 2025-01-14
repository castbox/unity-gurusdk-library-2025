#if UNITY_IOS
using System.Runtime.InteropServices;
using UnityEngine;

namespace Guru.Vibration
{
    internal class VibrationLibIOS : IVibrationLib
    {
        [DllImport("__Internal")]
        private static extern void initialize();

        [DllImport("__Internal")]
        private static extern bool hasVibrator();

        [DllImport("__Internal")]
        private static extern bool hasAmplitudeControl();

        [DllImport("__Internal")]
        private static extern bool hasCustomVibrationsSupport();

        [DllImport("__Internal")]
        private static extern bool hasVibrationEffect();

        [DllImport("__Internal")]
        private static extern void vibrate(int command, string jsonData);

        internal VibrationLibIOS()
        {
            initialize();
        }

        bool IVibrationLib.HasVibrator()
        {
            return hasVibrator();
        }

        bool IVibrationLib.HasAmplitudeControl()
        {
            return hasAmplitudeControl();
        }

        bool IVibrationLib.HasCustomVibrationsSupport()
        {
            return hasCustomVibrationsSupport();
        }

        bool IVibrationLib.HasVibrationEffect()
        {
            return hasVibrationEffect();
        }

        void IVibrationLib.Vibrate(VibrationCommand command, VibrateData vibrateData)
        {
            string jsonData = JsonUtility.ToJson(vibrateData);
            vibrate((int)command, jsonData);
        }
    }
}
#endif