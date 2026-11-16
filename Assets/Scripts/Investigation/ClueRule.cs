using System;
using UnityEngine;
using WrongSuspect.Interactables;

namespace WrongSuspect.Investigation
{
    /// <summary>
    /// One thing Sherlock checks at the end of the preparation. When its condition is true he says the line
    /// and adds the weight to one suspect. Edited in the Inspector on the level's CaseFile.
    /// </summary>
    [Serializable]
    public class ClueRule
    {
        public enum Condition { Always, ItemVisible, ItemNear, ObjectOpen, ObjectClosed, AlibiIs }

        [SerializeField, TextArea(1, 3)] private string line;
        [SerializeField] private Condition condition;

        [Header("What the condition looks at")]
        [SerializeField, Tooltip("ItemVisible / ItemNear: the evidence.")] private Carriable item;
        [SerializeField, Tooltip("ItemNear: the object it must be near.")] private Transform place;
        [SerializeField] private float radius = 1.5f;
        [SerializeField, Tooltip("ObjectOpen / ObjectClosed.")] private OpenableAnimator openable;
        [SerializeField, Tooltip("AlibiIs: 0 = first option.")] private int alibiIndex;

        [Header("What it means")]
        [SerializeField, Tooltip("Who this clue points at (empty = nobody).")] private SuspectProfile suspect;
        [SerializeField] private int weight;
        [SerializeField, Tooltip("A sign that the scene was arranged: a win with one of these is only partial.")]
        private bool contradiction;
        [SerializeField, Tooltip("Where Sherlock walks to examine it (empty = he just says it).")]
        private Transform lookAt;

        public string Line => line;
        public SuspectProfile Suspect => suspect;
        public int Weight => weight;
        public bool IsContradiction => contradiction;
        public Transform LookAt => lookAt;

        public bool IsTrue(int chosenAlibi)
        {
            switch (condition)
            {
                case Condition.Always:
                    return true;
                case Condition.ItemVisible:
                    return item != null && item.Current == Carriable.State.InWorld;
                case Condition.ItemNear:
                    return item != null && place != null && item.Current == Carriable.State.InWorld &&
                           Vector2.Distance(item.transform.position, place.position) <= radius;
                case Condition.ObjectOpen:
                    return openable != null && openable.IsOpen;
                case Condition.ObjectClosed:
                    return openable != null && !openable.IsOpen;
                case Condition.AlibiIs:
                    return chosenAlibi == alibiIndex;
                default:
                    return false;
            }
        }
    }
}
