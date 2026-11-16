using UnityEngine;

namespace WrongSuspect.Actions
{
    /// <summary>Puts the carried item down next to the chosen object (e.g. the glove by the desk).</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Actions/Place", fileName = "Place")]
    public class PlaceAction : GameAction
    {
        public override bool IsAvailable(ActionContext context) =>
            !context.Hands.IsEmpty && context.Target.AcceptsPlacedItems;

        public override void Execute(ActionContext context)
        {
            context.Hands.Release().PlaceAt(context.Target.DropPoint, context.Target);
        }
    }
}
