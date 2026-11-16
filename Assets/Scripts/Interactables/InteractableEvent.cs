using UnityEngine;
using WrongSuspect.Events;

namespace WrongSuspect.Interactables
{
    [CreateAssetMenu(menuName = "The Wrong Suspect/Events/Interactable Event", fileName = "NewInteractableEvent")]
    public class InteractableEvent : GameEvent<Interactable>
    {
    }
}
