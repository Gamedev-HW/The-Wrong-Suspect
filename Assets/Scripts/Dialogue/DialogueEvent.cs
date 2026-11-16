using UnityEngine;
using WrongSuspect.Events;

namespace WrongSuspect.Dialogue
{
    /// <summary>Raised for every line of speech (an empty line hides the dialogue box).</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Events/Dialogue Event", fileName = "NewDialogueEvent")]
    public class DialogueEvent : GameEvent<DialogueLine>
    {
    }
}
