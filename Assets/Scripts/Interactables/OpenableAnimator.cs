using UnityEngine;
using UnityEngine.Events;

namespace WrongSuspect.Interactables
{
    /// <summary>
    /// Plays the open/close animation of a door, window, wardrobe or drawer.
    /// Animator states: Closed, Opening (holds the open frame), Closing (holds the closed frame).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class OpenableAnimator : MonoBehaviour
    {
        public const string OpenParameter = "Open";
        public const string ClosedState = "Closed";
        public const string OpeningState = "Opening";
        public const string ClosingState = "Closing";

        private static readonly int OpenHash = Animator.StringToHash(OpenParameter);

        [SerializeField, Tooltip("Start the level already open, e.g. the wardrobe in level 1.")]
        private bool startOpen;
        [SerializeField, Tooltip("Raised with true when it opens and false when it closes (doors use it to clear the way).")]
        private UnityEvent<bool> openChanged = new UnityEvent<bool>();

        private Animator animator;

        public bool IsOpen { get; private set; }
        public UnityEvent<bool> OpenChanged => openChanged;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (startOpen)
            {
                SetOpen(true);
                animator.Play(OpeningState, 0, 1f);   // skip straight to the open frame
            }
        }

        public void Open() => SetOpen(true);

        public void Close() => SetOpen(false);

        public void Toggle() => SetOpen(!IsOpen);

        public void SetOpen(bool open)
        {
            IsOpen = open;
            animator.SetBool(OpenHash, open);
            openChanged.Invoke(open);
        }
    }
}
