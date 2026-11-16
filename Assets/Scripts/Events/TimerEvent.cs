using System;
using UnityEngine;

namespace WrongSuspect.Events
{
    /// <summary>
    /// An event asset that carries the countdown: the game timer raises it every frame,
    /// and UI such as the pocket watch listens without knowing who sent it.
    /// </summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Events/Timer Event", fileName = "NewTimerEvent")]
    public class TimerEvent : ScriptableObject
    {
        public event Action<float, float> Changed;

        public void Raise(float remainingSeconds, float totalSeconds) => Changed?.Invoke(remainingSeconds, totalSeconds);
    }
}
