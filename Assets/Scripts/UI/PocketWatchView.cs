using UnityEngine;
using UnityEngine.UI;
using WrongSuspect.Events;

namespace WrongSuspect.UI
{
    /// <summary>
    /// Shows the preparation countdown as a pocket watch. It listens to a TimerEvent asset,
    /// so the game timer never needs to know this UI exists.
    /// The minute hand turns once over the whole preparation time and the second hand ticks every second.
    /// Below the warning time the face turns red and the watch pulses.
    /// </summary>
    public class PocketWatchView : MonoBehaviour
    {
        public const string WarningParameter = "Warning";
        private static readonly int WarningHash = Animator.StringToHash(WarningParameter);

        [SerializeField] private TimerEvent timerChanged;
        [SerializeField] private float warningSeconds = 30f;

        [Header("Parts")]
        [SerializeField] private RectTransform minuteHand;
        [SerializeField] private RectTransform secondHand;
        [SerializeField] private Image face;
        [SerializeField] private Sprite normalFace;
        [SerializeField] private Sprite warningFace;
        [SerializeField] private Image warningGlow;
        [SerializeField] private Animator pulseAnimator;

        private bool warning;

        private void OnEnable()
        {
            if (timerChanged != null)
            {
                timerChanged.Changed += Show;
            }
        }

        private void OnDisable()
        {
            if (timerChanged != null)
            {
                timerChanged.Changed -= Show;
            }
        }

        private void Show(float remainingSeconds, float totalSeconds)
        {
            remainingSeconds = Mathf.Max(0f, remainingSeconds);
            float elapsed = Mathf.Max(0f, totalSeconds - remainingSeconds);
            float elapsed01 = totalSeconds > 0f ? elapsed / totalSeconds : 1f;

            minuteHand.localEulerAngles = new Vector3(0f, 0f, -360f * elapsed01);
            secondHand.localEulerAngles = new Vector3(0f, 0f, -6f * Mathf.Floor(elapsed));
            SetWarning(remainingSeconds > 0f && remainingSeconds <= warningSeconds);
        }

        private void SetWarning(bool value)
        {
            if (value == warning)
            {
                return;
            }
            warning = value;
            face.sprite = warning ? warningFace : normalFace;
            if (warningGlow != null)
            {
                warningGlow.enabled = warning;
            }
            if (pulseAnimator != null)
            {
                pulseAnimator.SetBool(WarningHash, warning);
            }
        }
    }
}
