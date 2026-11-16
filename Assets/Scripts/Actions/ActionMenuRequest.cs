using System.Collections.Generic;
using WrongSuspect.Interactables;

namespace WrongSuspect.Actions
{
    /// <summary>Everything the action menu needs to show: the object and the actions available on it right now.</summary>
    public class ActionMenuRequest
    {
        public ActionMenuRequest(Interactable target, IReadOnlyList<GameAction> actions, ActionContext context)
        {
            Target = target;
            Actions = actions;
            Context = context;
        }

        public Interactable Target { get; }
        public IReadOnlyList<GameAction> Actions { get; }
        public ActionContext Context { get; }
    }
}
