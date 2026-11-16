using UnityEngine;

namespace WrongSuspect.Interactables
{
    /// <summary>Shows a pulsing outline around an interactive object while the mouse is over it.</summary>
    public class InteractableHighlight : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer outline;
        [SerializeField] private HighlightStyle style;

        public bool IsShown { get; private set; }

        private void Awake()
        {
            Apply(false);
        }

        // Runs only while shown: Apply() switches this component on and off.
        private void Update()
        {
            if (!IsShown)
            {
                enabled = false;
                return;
            }
            float pulse = (Mathf.Sin(Time.time * style.PulseSpeed) + 1f) * 0.5f;
            Color c = style.Color;
            c.a *= Mathf.Lerp(style.MinAlpha, 1f, pulse);
            outline.color = c;
        }

        // Also runs when the object is picked up (deactivated) while highlighted.
        private void OnDisable()
        {
            IsShown = false;
            if (outline != null)
            {
                outline.enabled = false;
            }
        }

        public void Show() => Apply(true);

        public void Hide() => Apply(false);

        private void Apply(bool shown)
        {
            IsShown = shown && outline != null && style != null;
            if (outline != null)
            {
                outline.enabled = IsShown;
            }
            enabled = IsShown;
        }
    }
}
