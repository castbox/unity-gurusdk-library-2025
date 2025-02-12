namespace Guru.Vibration
{
    internal interface IVibrationLib
    {
        internal bool HasVibrator();
        internal bool HasAmplitudeControl();
        internal bool HasCustomVibrationsSupport();
        internal bool HasVibrationEffect();
        internal void Vibrate(VibrationCommand command, VibrateData vibrateData = default);
    }
}