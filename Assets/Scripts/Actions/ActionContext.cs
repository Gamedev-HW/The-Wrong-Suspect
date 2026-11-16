using WrongSuspect.Interactables;
using WrongSuspect.Player;

namespace WrongSuspect.Actions
{
    /// <summary>What an action works on: the chosen object and the culprit's hands.</summary>
    public class ActionContext
    {
        public ActionContext(Interactable target, PlayerHands hands)
        {
            Target = target;
            Hands = hands;
        }

        public Interactable Target { get; }
        public PlayerHands Hands { get; }

        public bool TryGetTarget<T>(out T component) where T : UnityEngine.Component => Target.TryGetComponent(out component);
    }
}
