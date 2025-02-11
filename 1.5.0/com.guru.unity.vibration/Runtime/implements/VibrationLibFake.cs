using UnityEngine;

#if UNITY_EDITOR
namespace Guru.Vibration
{
    internal class VibrationLibFake : IVibrationLib
    {
        bool IVibrationLib.HasVibrator()
        {
            return false;
        }

        bool IVibrationLib.HasAmplitudeControl()
        {
            return false;
        }

        bool IVibrationLib.HasCustomVibrationsSupport()
        {
            return false;
        }

        bool IVibrationLib.HasVibrationEffect()
        {
            return false;
        }

        void IVibrationLib.Vibrate(VibrationCommand command, VibrateData vibrateData)
        {
            string jsonData = JsonUtility.ToJson(vibrateData);
            Debug.Log("VibrationLibFake: Vibrate: " + jsonData);
        }
    }
}
#endif