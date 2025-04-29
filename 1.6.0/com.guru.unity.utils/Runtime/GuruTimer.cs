using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Guru
{
    public class GuruTimer
    {
        private const int TIMER_TICK_INTERVAL = 100;

        public int ElapsedMilliseconds => _elapsedMilliseconds;

        public int DurationMilliseconds => _durationMilliseconds;

        public bool IsRunning => _isRunning;

        // 计时器的Tick事件
        private readonly Action<int> _onTimerTick;

        // 计时器完成事件
        private readonly Action _onTimerComplete;

        // 每次重复的回调事件
        private readonly Action _onRepeat;

        // 计时器的总时长
        private readonly int _durationMilliseconds;

        // 计时器的重复次数
        private readonly int _repeatCount;

        // 取消Token的Source
        private CancellationTokenSource _cts;

        // 记录目前经过的时间
        private int _elapsedMilliseconds;

        // 计时器是否正在运行
        private bool _isRunning;

        // 当前重复次数
        private int _currentRepeatCount;

        public GuruTimer(int durationMilliseconds, int repeatCount = 1, Action<int> onTimerTick = null, Action onTimerComplete = null, Action onRepeat = null)
        {
            this._durationMilliseconds = durationMilliseconds;
            this._repeatCount = repeatCount;
            this._onTimerTick = onTimerTick;
            this._onTimerComplete = onTimerComplete;
            this._onRepeat = onRepeat;

            this._isRunning = false;
            this._currentRepeatCount = 0;
        }

        // 开始计时器
        public void Start()
        {
            if (this._isRunning)
            {
                return;
            }

            // 重置定时器状态
            this._isRunning = true;
            this._elapsedMilliseconds = 0;
            this._currentRepeatCount = 0; // 重置当前重复次数

            // 初始化 CancellationTokenSource
            this._cts = new CancellationTokenSource();

            // 开始计时器循环
            _ = TimerLoop();
        }

        // 停止计数器
        public void Stop()
        {
            if (!this._isRunning)
            {
                return;
            }

            this._isRunning = false;

            // 取消await UniTask.Delay()，中止计时器循环
            this._cts.Cancel();
        }

        private async UniTask TimerLoop()
        {
            while (this._currentRepeatCount < this._repeatCount && this._isRunning)
            {
                while (this._elapsedMilliseconds < this._durationMilliseconds && this._isRunning)
                {
                    await UniTask.Delay(TimeSpan.FromMilliseconds(TIMER_TICK_INTERVAL), cancellationToken: this._cts.Token);

                    this._elapsedMilliseconds += TIMER_TICK_INTERVAL;

                    this._onTimerTick?.Invoke(this._elapsedMilliseconds);
                }

                if (this._isRunning)
                {
                    // 调用每次重复的回调函数
                    this._onRepeat?.Invoke();

                    // 增加当前重复次数
                    this._currentRepeatCount++;

                    // 重置经过时间以便下次重复使用
                    this._elapsedMilliseconds = 0;
                }
            }

            // 计时器完成
            if (this._isRunning)
                this._onTimerComplete?.Invoke();

            this._isRunning = false;
        }
    }
}