using UnityEngine;
using UnityEngine.Pool;

namespace CustomTimers.Examples
{
    public class CustomTimerExample : MonoBehaviour
    {
        public float TimerTickRate = .3f;
        public float TimerDuration = 10.0f;


        CustomTimer timer = null;
        public void StartTimer()
        {
            timer = GenericPool<CustomTimer>.Get();
            timer?.Start(TimerDuration, OnCompleteCallBack, TimerTickRate, OnUpdateTimer);
        }
        public void PauseTimer()
        {
            timer?.Pause();

            ColoredDebug("Timer Paused ", "Red");
        }
        public void ResumeTimer()
        {
            timer?.Resume();

            ColoredDebug("Timer Resumed ", "Green");
        }
        public void StopTimer()
        {
            timer?.Cancel();
            if (timer != null)
                GenericPool<CustomTimer>.Release(timer);
            ColoredDebug("Timer Stopped ", "Orange");
        }

        private void OnCompleteCallBack()
        {
            ColoredDebug("Timer Completed ", "Green");
            GenericPool<CustomTimer>.Release(timer);
        }

        private void OnUpdateTimer(float obj)
        {
            //Debug.Log("Elapsed TIme  : " + obj);
            ColoredDebug("Timer Updating");
        }


        private void ColoredDebug(string text, string color = "White")
        {
            Debug.Log($"<color={color}> {text} </color>");
        }

      
        public async void CallingMultipleTimers()
        {
            var timer1 = GenericPool<CustomTimer>.Get();
            var timer2 = GenericPool<CustomTimer>.Get();
            var timer3 = GenericPool<CustomTimer>.Get();

            await timer1.Start(5, () =>
            {
                Debug.Log("Finished 1"); GenericPool<CustomTimer>.Release(timer1);
            });

            await timer2.Start(5, () =>
            {
                Debug.Log("Finished 2"); GenericPool<CustomTimer>.Release(timer2);
            });

            await timer3.Start(5, () =>
            {
                Debug.Log("Finished 3"); GenericPool<CustomTimer>.Release(timer3);
            });
        }
    }
}
