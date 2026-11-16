using System;
using UnityEngine;

namespace WrongSuspect.Events
{
    /// <summary>
    /// An event saved as an asset. The sender raises it and listeners react,
    /// without either side holding a reference to the other.
    /// </summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Events/Game Event", fileName = "NewGameEvent")]
    public class GameEvent : ScriptableObject
    {
        public event Action Raised;

        public void Raise() => Raised?.Invoke();
    }
}
