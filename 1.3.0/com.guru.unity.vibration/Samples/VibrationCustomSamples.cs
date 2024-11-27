using System;
using System.Globalization;
using System.Linq;
using Guru.Vibration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Samples
{
    [Serializable]
    public class VibrationCustomSamples : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField _patternsInputField;
        [SerializeField]
        private TMP_InputField _intensitiesInputField;

        private VibrateData _da_vibrateData;
        private VibrateData _pi_vibrateData;

        public void Start()
        {
            this._pi_vibrateData = new VibrateData(pattern: this._patternsInputField.text.Split(',').Select(int.Parse).ToList(), intensities: this._intensitiesInputField.text.Split(',').Select
                (float.Parse).ToList());
        }


        public void OnPatternsValueChanged()
        {
            try
            {
                this._pi_vibrateData.pattern = this._patternsInputField.text.Split(',').Select(int.Parse).ToList();
            }
            catch (Exception e)
            {
                Debug.LogError("只能输入整数！然后以,分隔");
            }
        }

        public void OnIntensitiesValueChanged()
        {
            try
            {
                this._pi_vibrateData.intensities = this._intensitiesInputField.text.Split(',').Select(float.Parse).ToList();
            }
            catch (Exception e)
            {
                Debug.LogError("只能输入数字！然后以,分隔");
            }
        }

        // Start duration+amplitude vibration
        public void Start_DA_Vibration()
        {
            VibrationService.CustomVibrate(this._da_vibrateData);
        }

        // Start patterns+intensities vibration
        public void Start_PI_Vibration()
        {
            VibrationService.CustomVibrate(this._pi_vibrateData);
        }
    }
}