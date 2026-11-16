using UnityEngine;
using WrongSuspect.Characters;

namespace WrongSuspect.Detective
{
    /// <summary>
    /// Plays Sherlock's investigation poses: inspecting, thinking and accusing.
    /// Walking is handled by <see cref="CharacterAnimator"/>.
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(CharacterAnimator))]
    public class DetectiveAnimator : MonoBehaviour
    {
        // Parameter name = animation name (Sherlock_Inspect_*, Sherlock_Think_Down, Sherlock_Accuse_*).
        public const string InspectParameter = "Inspect";
        public const string ThinkParameter = "Think";
        public const string AccuseParameter = "Accuse";

        private static readonly int InspectHash = Animator.StringToHash(InspectParameter);
        private static readonly int ThinkHash = Animator.StringToHash(ThinkParameter);
        private static readonly int AccuseHash = Animator.StringToHash(AccuseParameter);

        private Animator animator;
        private CharacterAnimator locomotion;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            locomotion = GetComponent<CharacterAnimator>();
        }

        public void StartInspecting() => animator.SetBool(InspectHash, true);

        public void StopInspecting() => animator.SetBool(InspectHash, false);

        public void StartThinking() => animator.SetBool(ThinkHash, true);

        public void StopThinking() => animator.SetBool(ThinkHash, false);

        /// <summary>Turns towards the accused and points; holds the pose until StopAccusing.</summary>
        public void AccuseAt(Transform suspect)
        {
            locomotion.FaceTowards(suspect);
            animator.SetBool(AccuseHash, true);
        }

        /// <summary>Points in the direction Sherlock is facing (e.g. at the player, when facing down).</summary>
        public void Accuse() => animator.SetBool(AccuseHash, true);

        public void StopAccusing() => animator.SetBool(AccuseHash, false);

        public void ResetPose()
        {
            animator.SetBool(InspectHash, false);
            animator.SetBool(ThinkHash, false);
            animator.SetBool(AccuseHash, false);
        }
    }
}
