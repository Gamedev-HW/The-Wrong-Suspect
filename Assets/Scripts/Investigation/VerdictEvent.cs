using UnityEngine;
using WrongSuspect.Events;

namespace WrongSuspect.Investigation
{
    /// <summary>Raised when Sherlock has reached his verdict; the results screen shows it.</summary>
    [CreateAssetMenu(menuName = "The Wrong Suspect/Events/Verdict Event", fileName = "NewVerdictEvent")]
    public class VerdictEvent : GameEvent<Verdict>
    {
    }
}
