using System;
using UnityEngine;

namespace WrongSuspect.Events
{
    /// <summary>
    /// Base for event assets that carry a value (a bool, a number, an object...).
    /// Each value type gets a tiny concrete subclass so Unity can create it as an asset.
    /// </summary>
    public abstract class GameEvent<T> : ScriptableObject
    {
        public event Action<T> Raised;

        public void Raise(T value) => Raised?.Invoke(value);
    }
}
