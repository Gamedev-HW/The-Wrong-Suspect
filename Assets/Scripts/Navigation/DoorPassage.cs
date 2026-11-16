using UnityEngine;
using WrongSuspect.Events;

namespace WrongSuspect.Navigation
{
    /// <summary>
    /// Blocks a doorway while the door is closed. The door's OpenableAnimator calls SetOpen (wired in the Inspector);
    /// the change is announced so the room's NavGrid rebuilds its paths.
    /// </summary>
    public class DoorPassage : MonoBehaviour
    {
        [SerializeField, Tooltip("Collider on the Obstacles layer that fills the doorway.")]
        private Collider2D blocker;

        [Header("Raises")]
        [SerializeField] private GameEvent obstaclesChanged;

        public bool IsOpen => blocker != null && !blocker.enabled;

        public void SetOpen(bool open)
        {
            if (blocker == null || blocker.enabled != open)
            {
                return;     // already in that state
            }
            blocker.enabled = !open;
            if (obstaclesChanged != null)
            {
                obstaclesChanged.Raise();
            }
        }
    }
}
