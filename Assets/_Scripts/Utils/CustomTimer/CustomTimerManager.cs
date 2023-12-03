using UnityEngine.Pool;
using System.Collections.Concurrent;
using System;
using UnityEngine;

namespace CustomTimers
{
    public class CustomTimerManager : MonoBehaviour
    {
        private ConcurrentDictionary<float, CustomUpdater> _customUpdates;
        private ConcurrentDictionary<float, CustomTimer> _customTimers;
        private static CustomTimerManager _instance;
        private CustomTimerManager()
        {
            _customUpdates = new ConcurrentDictionary<float, CustomUpdater>();
            _customTimers = new ConcurrentDictionary<float, CustomTimer>();
        }
        public static CustomTimerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(CustomTimerManager).Name);
                    _instance = obj.AddComponent<CustomTimerManager>();
                }
                return _instance;
            }
        }
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            StopAllTimers();
            StopAllUpdates();
        }

        #region Custom Updates
        /// <summary>
        /// Creates a new custom update with the specified update rate and callback function.
        /// If the update rate is recyclable and a custom update with the same rate already exists,
        /// the callback function is added to the existing custom update. Otherwise, a new custom update is created.
        /// </summary>
        /// <param name="updateRate">The update rate of the custom update.</param>
        /// <param name="onUpdate">The callback function to execute on each update.</param>
        /// <param name="recyclable">Whether the custom update can be reused.</param>
        /// <returns>The created or reused custom update.</returns>
        public CustomUpdater CreateUpdate(float updateRate, Action onUpdate, bool recyclable = true)
        {
            if (recyclable && _customUpdates.TryGetValue(updateRate, out CustomUpdater customUpdate))
            {
                if (!customUpdate.IsUpdateRunning)
                    customUpdate.Start(updateRate, onUpdate);
                else
                    customUpdate.OnUpdateCallback += onUpdate;

                return customUpdate;
            }

            var newCustomUpdate = GenericPool<CustomUpdater>.Get();
            _customUpdates.TryAdd(updateRate, newCustomUpdate);

            newCustomUpdate.Start(updateRate, onUpdate);
            newCustomUpdate.OnUpdateStopped += () =>
            {
                _customUpdates.TryRemove(updateRate, out _);
                GenericPool<CustomUpdater>.Release(newCustomUpdate);
            };

            return newCustomUpdate;
        }
        /// <summary>
        /// Pauses all active custom updates.
        /// This stops the execution of their update logic until they are resumed.
        /// </summary>
        public void PauseAllUpdates()
        {
            foreach (var update in _customUpdates.Values)
            {
                update.Pause();
            }
        }
        /// <summary>
        /// Resumes all paused custom updates.
        /// This allows the execution of their update logic to continue.
        /// </summary>
        public void ResumeAllUpdates()
        {
            foreach (var update in _customUpdates.Values)
            {
                update.Resume();
            }
        }
        /// <summary>
        /// Stops all active custom updates and removes them from the manager.
        /// </summary>
        public void StopAllUpdates()
        {
            foreach (var update in _customUpdates.Values)
            {
                update.Stop();
            }
            _customUpdates.Clear();
        }
        /// <summary>
        /// Pauses the specified custom update.
        /// This stops the execution of its update logic until it is resumed.
        /// </summary>
        /// <param name="updater">The custom update to pause.</param>
        public void PauseUpdate(CustomUpdater updater)
        {
            updater?.Pause();
        }
        /// <summary>
        /// Resumes the specified paused custom update.
        /// This allows the execution of its update logic to continue.
        /// </summary>
        /// <param name="updater">The custom update to resume.</param>
        public void ResumeUpdate(CustomUpdater updater)
        {
            updater?.Resume();
        }
        /// <summary>
        /// Stops the specified custom update and removes it from the manager.
        /// </summary>
        /// <param name="updater">The custom update to stop.</param>
        public void StopUpdate(CustomUpdater updater)
        {
            updater?.Stop();
        }

        #endregion
        #region Custom Timers
        /// <summary>
        /// Creates a new custom timer with the specified duration, complete callback function,
        /// timer tick rate, and optional update callback function.
        /// </summary>
        /// <param name="duration">The duration of the custom timer.</param>
        /// <param name="OnComplete">The callback function to execute when the timer completes.</param>
        /// <param name="TimerTickRate">The tick rate of the custom timer.</param>
        /// <param name="OnUpdate">The optional callback function to execute on each timer tick.</param>
        /// <returns>The created custom timer.</returns>
        public CustomTimer CreateTimer(float duration, Action OnComplete, float TimerTickRate = 1, Action<float> OnUpdate = null)
        {
            var newCustomTimer = GenericPool<CustomTimer>.Get();
            _customTimers.TryAdd(duration, newCustomTimer);

            newCustomTimer.Start(duration, OnComplete, TimerTickRate, OnUpdate);

            newCustomTimer.OnTimerStoppedCallback += () =>
            {
                newCustomTimer.OnTimerStoppedCallback = null;
                _customUpdates.TryRemove(duration, out _);
                GenericPool<CustomTimer>.Release(newCustomTimer);
            };

            return newCustomTimer;
        }
        /// <summary>
        /// Pauses all active custom timers.
        /// This stops the progression of their timer logic until they are resumed.
        /// </summary>
        public void PauseAllTimers()
        {
            foreach (var timer in _customTimers.Values)
            {
                timer?.Pause();
            }
        }
        /// <summary>
        /// Resumes all paused custom timers.
        /// This allows the progression of their timer logic to continue.
        /// </summary>
        public void ResumeAllTimers()
        {
            foreach (var timer in _customTimers.Values)
            {
                timer?.Resume();
            }
        }
        /// <summary>
        /// Stops all active custom timers and removes them from the manager.
        /// </summary>
        public void StopAllTimers()
        {
            foreach (var timer in _customTimers.Values)
            {
                timer?.Cancel();
            }
            _customTimers?.Clear();
        }
        /// <summary>
        /// Pauses the specified custom timer.
        /// This stops the progression of its timer logic until it is resumed.
        /// </summary>
        /// <param name="timer">The custom timer to pause.</param>
        public void PauseTimer(CustomTimer timer)
        {
            timer?.Pause();
        }
        /// <summary>
        /// Resumes the specified paused custom timer.
        /// This allows the progression of its timer logic to continue.
        /// </summary>
        /// <param name="timer">The custom timer to resume.</param>
        public void ResumeTimer(CustomTimer timer)
        {
            timer?.Resume();
        }
        /// <summary>
        /// Stops the specified custom timer and removes it from the manager.
        /// </summary>
        /// <param name="timer">The custom timer to stop.</param>
        public void StopTimer(CustomTimer timer)
        {
            timer?.Cancel();
        }
        #endregion
    }
}
