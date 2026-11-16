using UnityEngine;

namespace WrongSuspect.Player
{
    /// <summary>
    /// Plays the culprit's action animations: a short reach for pick-up/place,
    /// and a looping "working" pose while a timed action (hide, unlock) runs.
    /// Every method takes no arguments, so you can wire it to UnityEvents in the Inspector.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerActionAnimator : MonoBehaviour
    {
        // Parameter name = animation name (Player_Busy_*, Player_Interact_*).
        public const string BusyParameter = "Busy";
        public const string InteractParameter = "Interact";

        private static readonly int BusyHash = Animator.StringToHash(BusyParameter);
        private static readonly int InteractHash = Animator.StringToHash(InteractParameter);

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void PlayInteract() => animator.SetTrigger(InteractHash);

        public void StartBusy() => animator.SetBool(BusyHash, true);

        public void StopBusy() => animator.SetBool(BusyHash, false);
    }
}
