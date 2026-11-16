using UnityEngine;
using WrongSuspect.Interactables;

namespace WrongSuspect.Actions
{
    /// <summary>Looks at the object: raises an event so the UI shows its description.</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Actions/Inspect", fileName = "Inspect")]
    public class InspectAction : GameAction
    {
        [SerializeField] private InteractableEvent inspected;

        public override bool IsAvailable(ActionContext context) => true;

        public override void Execute(ActionContext context)
        {
            if (inspected != null)
            {
                inspected.Raise(context.Target);
            }
        }
    }
}
