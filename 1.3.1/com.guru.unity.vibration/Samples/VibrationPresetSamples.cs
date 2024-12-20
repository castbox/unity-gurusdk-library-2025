using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guru.Vibration;

public class VibrationPresetSamples : MonoBehaviour
{
    public void Success()
    {
        VibrationService.Success();
    }

    public void Error()
    {
        VibrationService.Error();
    }

    public void Warning()
    {
        VibrationService.Warning();
    }

    public void Light()
    {
        VibrationService.Light();
    }

    public void Medium()
    {
        VibrationService.Medium();
    }

    public void Heavy()
    {
        VibrationService.Heavy();
    }

    public void Double()
    {
        VibrationService.Double();
    }

    public void Selection()
    {
        VibrationService.Selection();
    }

    public void Cancel()
    {
        VibrationService.CancelVibrate();
    }
}
