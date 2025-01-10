using System;
using System.Collections;
using NUnit.Framework;
using Guru;
using UnityEngine;
using UnityEngine.TestTools;

namespace GuruCore.Tests
{
    [TestFixture]
    [Category("Guru/Runtime/GuruSDKCore/GuruTimer")]
    public class TimerTests
    {
        private bool _timerComplete = false;
        private GuruTimer _timer;

        [OneTimeSetUp, Order(1)]
        public void SetUp()
        {
            void OnCompleted()
            {
                Debug.Log("Timer completed");
                this._timerComplete = true;
            }

            this._timer = new GuruTimer(3000, onTimerComplete: OnCompleted);
        }

        [UnityTest, Order(2), Description("测试正常运行")]
        public IEnumerator Test_Normal()
        {
            // 测试正常运行
            this._timer.Start();
            yield return new WaitForSeconds(4);
            this._timer.Stop();
            Assert.IsTrue(_timerComplete);
        }

        [UnityTest, Order(3), Description("测试提前停止")]
        public IEnumerator Test_Suspend()
        {
            _timerComplete = false;
            this._timer.Start();
            yield return new WaitForSeconds(2);
            this._timer.Stop();
            Assert.IsTrue(!_timerComplete);
        }

        [UnityTest, Order(4), Description("再次测试正常运行")]
        public IEnumerator Test_Normal2()
        {
            _timerComplete = false;
            this._timer.Start();
            yield return new WaitForSeconds(4);
            this._timer.Stop();
            Assert.IsTrue(_timerComplete);
        }
    }
}
