namespace CustomTimers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public class CustomTimer
    {
        private CancellationTokenSource cancellationTokenSource;
        private PauseTokenSource pauseTokenSource;
        private bool timerRunning;


        internal Action OnCompleteCallback;
        internal Action<float> OnUpdateCallback;
        internal Action OnTimerStoppedCallback;

        /// <summary>
        /// Starts a timer with the specified duration and callbacks.
        /// </summary>
        /// <param name="duration">The duration of the timer in seconds.</param>
        /// <param name="OnComplete">Callback invoked when the timer completes.</param>
        /// <param name="TimerTickRate">The update rate of the timer in seconds.</param>
        /// <param name="OnUpdate">Callback invoked whenever the timer updates.</param>
        /// <returns>A Task representing the asynchronous timer operation.</returns>
        internal async Task Start(float duration, Action OnComplete, float TimerTickRate = 1, Action<float> OnUpdate = null)
        {
            if (timerRunning) return;

            cancellationTokenSource = new CancellationTokenSource();
            pauseTokenSource = new PauseTokenSource();

            OnCompleteCallback += OnComplete;
            OnUpdateCallback += OnUpdate;

            timerRunning = true;
            var elapsedTime = 0.0f;

            while (elapsedTime < duration)
            {
                await pauseTokenSource.Token.WaitWhilePausedAsync(cancellationTokenSource.Token);

                if (cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                elapsedTime += TimerTickRate;

                int waitTimeInMilliSeconds = (int)(TimerTickRate * 1000); // Converting seconds to milliseconds
                OnUpdateCallback?.Invoke(elapsedTime);

                await Task.Delay(waitTimeInMilliSeconds);

            }
            timerRunning = false;
            if (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                OnCompleteCallback?.Invoke();
            }
            OnTimerStoppedCallback?.Invoke();
        }

        /// <summary>
        /// Pauses the timer.
        /// </summary>
        internal void Pause()
        {
            pauseTokenSource.Pause();
        }

        /// <summary>
        /// Resumes the timer.
        /// </summary>
        internal void Resume()
        {
            pauseTokenSource.Resume();
        }

        /// <summary>
        /// Cancels the timer.
        /// </summary>
        internal void Cancel()
        {
            Debug.Log("Canceling Timer");
            cancellationTokenSource.Cancel();
        }
    }

    public class CustomUpdater
    {
        private CancellationTokenSource cancellationTokenSource;
        private PauseTokenSource pauseTokenSource;
        private bool updaterRunning;

        internal Action OnUpdateCallback = null;
        internal Action OnUpdateStopped = null;
        /// <summary>
        /// Starts the custom update with the specified update rate and callbacks.
        /// </summary>
        /// <param name="updateTickRate">The rate of update in seconds.</param>
        /// <param name="OnUpdate">Callback invoked on each update.</param>
        /// <returns>A Task representing the asynchronous update operation.</returns>
        internal async Task Start(float updateTickRate, Action OnUpdate)
        {
            if (updaterRunning) return;

            cancellationTokenSource = new CancellationTokenSource();
            pauseTokenSource = new PauseTokenSource();
            OnUpdateCallback += OnUpdate;
            updaterRunning = true;

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                await pauseTokenSource.Token.WaitWhilePausedAsync(cancellationTokenSource.Token);

                OnUpdateCallback?.Invoke();

                int waitTimeInMilliSeconds = (int)(updateTickRate * 1000); // Converting seconds to milliseconds
                await Task.Delay(waitTimeInMilliSeconds);
            }

            OnUpdateStopped?.Invoke();
            updaterRunning = false;
        }

        /// <summary>
        /// Pauses the custom update.
        /// </summary>
        internal void Pause()
        {
            pauseTokenSource.Pause();
        }

        /// <summary>
        /// Resumes the custom update.
        /// </summary>
        internal void Resume()
        {
            pauseTokenSource.Resume();
        }

        /// <summary>
        /// Stops the custom update.
        /// </summary>
        internal void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }

    #region Pause Helper 
    public class PauseToken
    {
        private readonly PauseTokenSource source;

        public PauseToken(PauseTokenSource source)
        {
            this.source = source;
        }

        /// <summary>
        /// Asynchronously waits while the timer is paused.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public Task WaitWhilePausedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return source.WaitWhilePausedAsync(cancellationToken);
        }
    }

    public class PauseTokenSource
    {
        private bool isPaused;
        private TaskCompletionSource<bool> pauseRequestTcs;

        /// <summary>
        /// Gets the pause token associated with this pause token source.
        /// </summary>
        public PauseToken Token { get { return new PauseToken(this); } }

        /// <summary>
        /// Initializes a new instance of the PauseTokenSource class.
        /// </summary>
        public PauseTokenSource()
        {
            isPaused = false;
            pauseRequestTcs = new TaskCompletionSource<bool>();
        }

        /// <summary>
        /// Pauses the timer.
        /// </summary>
        public void Pause()
        {
            isPaused = true;
        }
        /// <summary>
        /// Resumes the timer.
        /// </summary>
        public void Resume()
        {
            isPaused = false;
            pauseRequestTcs.TrySetResult(true);
        }

        /// <summary>
        /// Asynchronously waits while the timer is paused.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task WaitWhilePausedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            while (isPaused)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await Task.Delay(10, cancellationToken);
            }

            //cancellationToken.ThrowIfCancellationRequested();
        }
    }
    #endregion
}