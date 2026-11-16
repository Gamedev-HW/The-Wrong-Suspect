using UnityEngine;

namespace WrongSuspect.Characters
{
    /// <summary>
    /// Turns the character's movement into the matching 4-direction idle/walk animation.
    /// It measures how far the transform moved, so it works with any movement code.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimator : MonoBehaviour
    {
        public const string MoveXParameter = "MoveX";
        public const string MoveYParameter = "MoveY";
        public const string LastXParameter = "LastX";
        public const string LastYParameter = "LastY";
        public const string SpeedParameter = "Speed";

        private static readonly int MoveXHash = Animator.StringToHash(MoveXParameter);
        private static readonly int MoveYHash = Animator.StringToHash(MoveYParameter);
        private static readonly int LastXHash = Animator.StringToHash(LastXParameter);
        private static readonly int LastYHash = Animator.StringToHash(LastYParameter);
        private static readonly int SpeedHash = Animator.StringToHash(SpeedParameter);

        [SerializeField] private Vector2 startFacing = Vector2.down;
        [SerializeField, Tooltip("Units per second below which the character counts as standing still.")]
        private float moveThreshold = 0.05f;

        private Animator animator;
        private Vector3 lastPosition;

        /// <summary>The direction the character faces: up, down, left or right.</summary>
        public Vector2 Facing { get; private set; } = Vector2.down;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            lastPosition = transform.position;
            SetFacing(startFacing);
        }

        private void LateUpdate()
        {
            Vector2 delta = transform.position - lastPosition;
            lastPosition = transform.position;

            float speed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            bool moving = speed > moveThreshold;
            if (moving)
            {
                SetFacing(delta);
            }

            Vector2 move = moving ? Facing : Vector2.zero;
            animator.SetFloat(MoveXHash, move.x);
            animator.SetFloat(MoveYHash, move.y);
            animator.SetFloat(SpeedHash, moving ? speed : 0f);
        }

        /// <summary>Turns the character. Diagonals snap to the stronger axis.</summary>
        public void SetFacing(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Facing = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? new Vector2(Mathf.Sign(direction.x), 0f)
                : new Vector2(0f, Mathf.Sign(direction.y));

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            animator.SetFloat(LastXHash, Facing.x);
            animator.SetFloat(LastYHash, Facing.y);
        }

        public void FaceTowards(Transform target) => SetFacing(target.position - transform.position);
    }
}
