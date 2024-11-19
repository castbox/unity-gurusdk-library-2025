using System;
using System.Collections;
using NUnit.Framework;
using Guru;
using UnityEngine;
using UnityEngine.TestTools;

namespace GuruCore.Tests
{
    [TestFixture]
    [Category("GuruSDKCore/GuruTimer")]
    public class TestTimer
    {
        private bool _timerComplete = false;

        [UnityTest]
        public IEnumerator Test_Timer_Basic()
        {
            void OnCompleted()
            {
                Debug.Log("Timer completed");
                _timerComplete = true;
            }

            GuruTimer timer = new GuruTimer(3, onTimerComplete: OnCompleted);

            // 测试正常运行
            timer.Start();
            yield return new WaitForSeconds(4);
            timer.Stop();
            Assert.IsTrue(_timerComplete);

            // 测试提前停止
            _timerComplete = false;
            timer.Start();
            yield return new WaitForSeconds(2);
            timer.Stop();
            Assert.IsTrue(!_timerComplete);

            // 再次测试正常运行
            _timerComplete = false;
            timer.Start();
            yield return new WaitForSeconds(4);
            timer.Stop();
            Assert.IsTrue(_timerComplete);
        }
    }
}
