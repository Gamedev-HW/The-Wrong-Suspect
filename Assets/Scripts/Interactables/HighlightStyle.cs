using UnityEngine;

namespace WrongSuspect.Interactables
{
    /// <summary>How interactive objects look when highlighted. All of them share one asset (Data/Settings/HighlightStyle).</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Highlight Style", fileName = "HighlightStyle")]
    public class HighlightStyle : ScriptableObject
    {
        [SerializeField] private Color color = new Color(1f, 0.84f, 0.37f, 1f);
        [SerializeField, Tooltip("Pulses per second (radians).")] private float pulseSpeed = 4f;
        [SerializeField, Range(0f, 1f)] private float minAlpha = 0.35f;

        public Color Color => color;
        public float PulseSpeed => pulseSpeed;
        public float MinAlpha => minAlpha;
    }
}
