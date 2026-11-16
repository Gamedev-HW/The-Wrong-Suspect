using UnityEngine;
using WrongSuspect.Interactables;

namespace WrongSuspect.Actions
{
    /// <summary>Opens or closes a door, window, wardrobe or drawer (not while it is still locked).</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Actions/Open or Close", fileName = "OpenClose")]
    public class OpenCloseAction : GameAction
    {
        [SerializeField] private string closeLabel = "Close";

        public override string LabelFor(ActionContext context) =>
            context.TryGetTarget(out OpenableAnimator openable) && openable.IsOpen ? closeLabel : base.LabelFor(context);

        public override bool IsAvailable(ActionContext context) =>
            context.TryGetTarget(out OpenableAnimator _) &&
            !(context.TryGetTarget(out Container container) && container.IsLocked);

        public override void Execute(ActionContext context)
        {
            if (context.TryGetTarget(out OpenableAnimator openable))
            {
                openable.Toggle();
            }
        }
    }
}
