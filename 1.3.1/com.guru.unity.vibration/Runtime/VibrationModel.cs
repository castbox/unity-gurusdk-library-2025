using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Guru.Vibration
{
    internal enum VibrationCommand
    {
        // Execute the vibration effect.
        Vibrate = 0,

        // Stop the vibration effect.
        Cancel
    }

    [System.Serializable]
    public enum VibrationStyle
    {
        /// Indicates that a task or action has completed.
        Success,

        /// Indicates that a task or action has produced a warning of some kind.
        Warning,

        /// Indicates that an error has occurred.
        Error,

        /// Indicates a collision between small or lightweight UI objects.
        Light,

        /// Indicates a collision between medium-sized or medium-weight UI objects.
        Medium,

        /// Indicates a collision between large or heavyweight UI objects.
        Heavy,

        /// Indicates that a UI element has been tapped or clicked Twice.
        Double,

        /// Indicates that a UI element’s values are changing.
        Selection,

        // indicates a custom vibration pattern.
        Custom
    }

    /// Vibrate with [pattern] at [intensities].
    /// The pattern is a list of vibration durations in milliseconds.
    /// The intensities is a list of amplitudes from 1 to 255.
    [System.Serializable]
    public struct VibrateData
    {
        public string style;
        public List<int> pattern;
        public List<int> intensities;

        public VibrateData(
            List<int> pattern = default,
            List<int> intensities = default,
            VibrationStyle style = VibrationStyle.Custom)
        {
            this.style = style.ToString();
            this.pattern = pattern ?? new List<int>();
            this.intensities = intensities ?? new List<int>();
        }
    }
}