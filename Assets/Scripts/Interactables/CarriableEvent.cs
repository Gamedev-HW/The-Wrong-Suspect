using UnityEngine;
using WrongSuspect.Events;

namespace WrongSuspect.Interactables
{
    /// <summary>Raised with the item now in the culprit's hands, or null when his hands are empty.</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Events/Carriable Event", fileName = "NewCarriableEvent")]
    public class CarriableEvent : GameEvent<Carriable>
    {
    }
}
