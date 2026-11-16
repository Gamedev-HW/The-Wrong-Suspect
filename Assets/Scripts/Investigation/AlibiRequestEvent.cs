using UnityEngine;
using WrongSuspect.Events;

namespace WrongSuspect.Investigation
{
    /// <summary>Raised with the alibi options when the culprit must choose one.</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Events/Alibi Request Event", fileName = "NewAlibiRequestEvent")]
    public class AlibiRequestEvent : GameEvent<string[]>
    {
    }
}
