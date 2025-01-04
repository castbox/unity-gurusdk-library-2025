using System;
using System.Collections;
using System.Collections.Generic;
using Guru.FacebookUnitySDK;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

public class GuruFacebookUniTest
{
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return null;
    }

    [Test]
    public void LogEvent()
    {
        GuruFacebook.Instance.Init((success) =>
        {
            Assert.IsTrue(success);
            GuruFacebook.Instance.LogAppEvent("TestEvent");
        });
    }
}