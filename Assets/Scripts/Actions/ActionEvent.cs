using UnityEngine;
using WrongSuspect.Events;

namespace WrongSuspect.Actions
{
    /// <summary>Raised by the action menu with the action the player clicked.</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Events/Action Event", fileName = "NewActionEvent")]
    public class ActionEvent : GameEvent<GameAction>
    {
    }
}
