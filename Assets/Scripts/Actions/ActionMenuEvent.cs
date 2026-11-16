using UnityEngine;
using WrongSuspect.Events;

namespace WrongSuspect.Actions
{
    /// <summary>Raised when the culprit reaches an object, asking the UI to show its action menu.</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Events/Action Menu Event", fileName = "NewActionMenuEvent")]
    public class ActionMenuEvent : GameEvent<ActionMenuRequest>
    {
    }
}
