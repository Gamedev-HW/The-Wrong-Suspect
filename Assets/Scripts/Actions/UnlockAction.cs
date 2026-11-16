using UnityEngine;
using WrongSuspect.Interactables;

namespace WrongSuspect.Actions
{
    /// <summary>Forces a locked container open (the 30-second "complex action" in level 3).</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Actions/Unlock", fileName = "Unlock")]
    public class UnlockAction : GameAction
    {
        public override bool IsAvailable(ActionContext context) =>
            context.TryGetTarget(out Container container) && container.IsLocked;

        public override void Execute(ActionContext context)
        {
            if (!context.TryGetTarget(out Container container))
            {
                return;
            }
            container.Unlock();
            if (context.TryGetTarget(out OpenableAnimator openable))
            {
                openable.Open();
            }
        }
    }
}
