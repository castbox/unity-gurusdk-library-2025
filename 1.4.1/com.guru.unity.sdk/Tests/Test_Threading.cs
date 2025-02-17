

namespace Guru.Tests
{
    using NUnit.Framework;
    using System.Threading;
    using UnityEngine;
    
    public class Test_Threading
    {

        private int TestCount
        {
            get => PlayerPrefs.GetInt(nameof(TestCount), 0);
            set => PlayerPrefs.SetInt(nameof(TestCount), value);
        }


        [Test]
        public void Test_ThreadingCall()
        {
            GuruSDK.Init(success =>
            {
                GuruSDK.Delay(0.1f, () =>
                {
                    CallThreading();
                });
            });
        }

        private void CallThreading()
        {
            Debug.Log($"--------- CallThreading -------------");
            var t = new Thread(() =>
            {
                Debug.Log($"--------- Thread Start -------------");
                Thread.Sleep(2000);
                GuruSDK.RunOnMainThread(() =>
                {
                    TestCount++;
                    Debug.Log($">>>>> CallThreading: {TestCount}");
                });
            });
            
            t.Start();
        }

    }
}