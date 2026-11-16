using UnityEngine;

namespace WrongSuspect.Actions
{
    /// <summary>
    /// One thing the culprit can do to an object (inspect, pick up, hide...).
    /// Each action is an asset in Data/Actions, so its label, icon and time cost are edited in the Inspector.
    /// To add a new kind of action, add a subclass and create an asset of it.
    /// </summary>
    public abstract class GameAction : ScriptableObject
    {
        [SerializeField] private string label = "Action";
        [SerializeField] private Sprite icon;
        [SerializeField, Tooltip("Seconds taken off the preparation clock.")]
        private float timeCost = 5f;
        [SerializeField, Tooltip("Real seconds the culprit's animation plays before the action happens.")]
        private float animationSeconds = 0.6f;
        [SerializeField, Tooltip("Loop the 'busy' animation instead of the short reach.")]
        private bool busyAnimation;

        public Sprite Icon => icon;
        public float TimeCost => timeCost;
        public float AnimationSeconds => animationSeconds;
        public bool UsesBusyAnimation => busyAnimation;

        public virtual string LabelFor(ActionContext context) => label;

        public abstract bool IsAvailable(ActionContext context);

        public abstract void Execute(ActionContext context);
    }
}
