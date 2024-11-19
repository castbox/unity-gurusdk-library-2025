using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Guru
{
    public class GuruTimer
    {
        public float ElapsedTime => _elapsedTime;

        public float Duration => _duration;

        public bool IsRunning => _isRunning;

        // 计时器的Tick事件
        private readonly Action<float> _onTimerTick;

        // 计时器完成事件
        private readonly Action _onTimerComplete;

        // 計時器的總時間
        private readonly float _duration;

        // 取消Token的Source
        private CancellationTokenSource _cts;

        // 记录目前经过的时间
        private float _elapsedTime;

        // 计时器是否正在運行
        private bool _isRunning;

        public GuruTimer(float duration, Action<float> onTimerTick = null, Action onTimerComplete = null)
        {
            this._duration = duration;
            this._onTimerTick = onTimerTick;
            this._onTimerComplete = onTimerComplete;

            this._isRunning = false;
        }

        // 开始计时器
        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            // 重置定时器状态
            this._isRunning = true;
            this._elapsedTime = 0f;

            // 初始化 CancellationTokenSource
            this._cts = new CancellationTokenSource();

            // 开始计时器循环
            _ = TimerLoop();
        }

        // 停止计数器
        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            this._isRunning = false;

            // 取消await UniTask.Delay()，中止计时器循环
            this._cts.Cancel();
        }

        private async UniTask TimerLoop()
        {
            // 计时器循环
            while (ElapsedTime < Duration && IsRunning)
            {
                // 等待 0.1 秒
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: this._cts.Token);

                this._elapsedTime += 0.1f;

                this._onTimerTick?.Invoke(ElapsedTime);
            }

            // 计时器完成
            if (IsRunning)
                this._onTimerComplete?.Invoke();

            this._isRunning = false;
        }
    }
}