using System.Collections.Generic;
using UnityEngine;

namespace WrongSuspect.Interactables
{
    /// <summary>A hiding place (wardrobe, drawer, cabinet) that remembers what was hidden in it.</summary>
    public class Container : MonoBehaviour
    {
        [SerializeField, Tooltip("Must be forced open (Unlock action) before anything can be hidden in it.")]
        private bool startsLocked;

        private readonly List<Carriable> contents = new List<Carriable>();

        public bool IsLocked { get; private set; }
        public IReadOnlyList<Carriable> Contents => contents;

        private void Awake()
        {
            IsLocked = startsLocked;
        }

        public void Unlock() => IsLocked = false;

        public void Store(Carriable item)
        {
            contents.Add(item);
            item.HideIn(this);
        }
    }
}
