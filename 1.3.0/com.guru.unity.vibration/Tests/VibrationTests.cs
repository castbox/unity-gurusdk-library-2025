using System.Collections;
using System.Collections.Generic;
using Guru.Vibration;
using UnityEngine;
using NUnit.Framework;

[Category("Guru/Runtime/Vibration")]
public class VibrationTests
{
    [OneTimeSetUp]
    public void Setup()
    {

    }

    [Test, Description("test 是否支持震动功能")]
    public void Test_HasVibrator()
    {
        bool hasVibration = VibrationService.HasHasVibrator();
        Assert.IsTrue(hasVibration);
    }

    [Test, Description("test 是否支持振幅控制")]
    public void Test_HasAmplitudeControl()
    {
        bool hasAmplitudeControl = VibrationService.HasAmplitudeControl();
        Assert.IsTrue(hasAmplitudeControl);
    }

    [Test, Description("test 是否支持自定义震动")]
    public void Test_HasCustomVibrationsSupport()
    {
        bool hasCustomVibrationsSupport = VibrationService.HasCustomVibrationsSupport();
#if UNITY_ANDROID
        Assert.IsTrue(hasCustomVibrationsSupport);
#elif UNITY_IOS
        Assert.IsFalse(hasCustomVibrationsSupport);
#endif
    }

    [Test, Description("test 是否支持震动效果, 只支持:" +
                       "1. 高通芯片； " +
                       "2. 不是ViVo的手机； " +
                       "3. 不是低内存的手机;" +
                       "4. 支持EFFECT_TICK, EFFECT_CLICK, EFFECT_HEAVY_CLICK效果的手机")]
    public void Test_HasVibrationEffect()
    {
        bool hasVibrationEffect = VibrationService.HasVibrationEffect();
        Assert.IsTrue(hasVibrationEffect);
    }

    [Test, Description("test Success震动一次")]
    public void Test_Vibrate_Success()
    {
        VibrationService.Success();
    }

    [Test, Description("test Warning震动一次")]
    public void Test_Vibrate_Warning()
    {
        VibrationService.Warning();
    }

    [Test, Description("test Error震动一次")]
    public void Test_Vibrate_Error()
    {
        VibrationService.Error();
    }
    [Test, Description("test Light震动一次")]
    public void Test_Vibrate_Light()
    {
        VibrationService.Light();
    }

    [Test, Description("test Medium震动一次")]
    public void Test_Vibrate_Medium()
    {
        VibrationService.Medium();
    }

    [Test, Description("test Heavy震动一次")]
    public void Test_Vibrate_Heavy()
    {
        VibrationService.Heavy();
    }

    [Test, Description("test Selection震动一次")]
    public void Test_Vibrate_Selection()
    {
        VibrationService.Selection();
    }

    [Test, Description("test Cancel震动一次")]
    public void Test_Vibrate_Cancel()
    {
        VibrationService.CancelVibrate();
    }
}