#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Guru
{

    public delegate void TimerElapsedHandler();

    public class UniTimer : IDisposable
    {
        private CancellationTokenSource? _cts;
        private readonly TimeSpan _duration;
        private readonly bool _isPeriodic;
        private readonly TimerElapsedHandler? _elapsed;
        private bool _isDisposed;
        private readonly object _lock = new();


        private bool IsActiveInternal()
        {
            return !_isDisposed && _cts is { Token: { IsCancellationRequested: false } };
        }

        public bool IsActive
        {
            get
            {
                lock (_lock)
                {
                    return IsActiveInternal();
                }
            }
        }


        private UniTimer(TimeSpan duration, bool isPeriodic, TimerElapsedHandler elapsed)
        {
            _duration = duration;
            _isPeriodic = isPeriodic;
            _elapsed = elapsed;
        }

        public static UniTimer Delayed(TimeSpan duration, TimerElapsedHandler elapsed)
        {
            return new UniTimer(duration, false, elapsed);
        }


        public static UniTimer Periodic(TimeSpan interval, TimerElapsedHandler elapsed)
        {
            return new UniTimer(interval, true, elapsed);
        }

        private async UniTaskVoid StartInternal(CancellationToken token)
        {
            try
            {
                do
                {
                    Debug.Log("Timer tick at: " + System.DateTime.UtcNow);

                    await UniTask.Delay(Convert.ToInt32(_duration.TotalMilliseconds), cancellationToken: token);

                    if (token.IsCancellationRequested)
                    {
                        Debug.Log("Timer cancelled!");
                        break;
                    }

                    _elapsed?.Invoke();
                } while (_isPeriodic && !token.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Timer cancelled (OperationCanceledException)!");
            }
        }

        public void Start()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    Debug.Log("Timer disposed.");
                    return;
                }

                if (IsActiveInternal())
                {
                    Debug.LogWarning("Timer already started.");
                    return;
                }

                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                StartInternal(_cts.Token).Forget();
                Debug.Log("Timer started.");
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed) return;

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
                _isDisposed = true;
            }
        }
    }
}