#if UNITY_ANDROID
using System;
using UnityEngine;

namespace Guru.Vibration
{
    internal class VibrationLibAndroid : IVibrationLib
    {
        public const string LOG_TAG = "[Vibration][A]";
        private const string VIBRATION_HANDLER_CLASS_NAME = "com.guru.unity.utility.VibrationHandler";
        private const string UNITY_PLAYER_CLASS_NAME = "com.unity3d.player.UnityPlayer";

        private readonly AndroidJavaObject _currentActivity;
        private readonly AndroidJavaObject _vibrationHandlerInst;

        internal VibrationLibAndroid()
        {
            using (var unityPlayer = new AndroidJavaClass(UNITY_PLAYER_CLASS_NAME))
            {
                this._currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
            using (var connectivity = new AndroidJavaClass(VIBRATION_HANDLER_CLASS_NAME))
            {
                this._vibrationHandlerInst = connectivity.CallStatic<AndroidJavaObject>("getInstance");
            }
            Initialize();
        }

        bool IVibrationLib.HasVibrator()
        {
            bool hasVibrator = this._vibrationHandlerInst.Call<bool>("hasVibrator");
            return hasVibrator;
        }

        bool IVibrationLib.HasAmplitudeControl()
        {
            return this._vibrationHandlerInst.Call<bool>("hasAmplitudeControl");
        }

        bool IVibrationLib.HasCustomVibrationsSupport()
        {
            return this._vibrationHandlerInst.Call<bool>("hasCustomVibrationsSupport");
        }

        bool IVibrationLib.HasVibrationEffect()
        {
            return this._vibrationHandlerInst.Call<bool>("hasVibrationEffect");
        }

        void IVibrationLib.Vibrate(VibrationCommand command, VibrateData vibrateData)
        {
            string jsonData = JsonUtility.ToJson(vibrateData);
            this._vibrationHandlerInst.Call("vibrate", command.ToString(), jsonData);
        }

        private void Initialize()
        {
            this._vibrationHandlerInst.Call("initialize", this._currentActivity);
        }
    }
}
#endif