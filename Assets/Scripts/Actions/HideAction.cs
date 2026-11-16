using UnityEngine;
using WrongSuspect.Interactables;

namespace WrongSuspect.Actions
{
    /// <summary>Hides the carried item inside an unlocked container (wardrobe, drawer, cabinet).</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Actions/Hide", fileName = "Hide")]
    public class HideAction : GameAction
    {
        public override bool IsAvailable(ActionContext context) =>
            !context.Hands.IsEmpty && context.TryGetTarget(out Container container) && !container.IsLocked;

        public override void Execute(ActionContext context)
        {
            if (context.TryGetTarget(out Container container))
            {
                container.Store(context.Hands.Release());
            }
        }
    }
}
