using UnityEngine;

namespace WrongSuspect.Investigation
{
    /// <summary>Someone Sherlock can accuse: name and portrait for the results screen (Data/Suspects).</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Suspect", fileName = "NewSuspect")]
    public class SuspectProfile : ScriptableObject
    {
        [SerializeField] private string displayName = "Suspect";
        [SerializeField] private Sprite portrait;
        [SerializeField, Tooltip("Tick only for the player's character: being accused means losing.")]
        private bool isCulprit;

        public string DisplayName => displayName;
        public Sprite Portrait => portrait;
        public bool IsCulprit => isCulprit;
    }
}
