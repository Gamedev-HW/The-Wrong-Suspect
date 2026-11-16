using UnityEngine;
using WrongSuspect.Interactables;

namespace WrongSuspect.Actions
{
    /// <summary>Takes a movable piece of evidence into the culprit's (empty) hands.</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Actions/Pick Up", fileName = "PickUp")]
    public class PickUpAction : GameAction
    {
        public override bool IsAvailable(ActionContext context) =>
            context.Hands.IsEmpty && context.TryGetTarget(out Carriable item) && item.Current == Carriable.State.InWorld;

        public override void Execute(ActionContext context)
        {
            if (context.TryGetTarget(out Carriable item))
            {
                context.Hands.Take(item);
            }
        }
    }
}
