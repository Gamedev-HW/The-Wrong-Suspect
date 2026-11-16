using UnityEngine;
using UnityEngine.Events;

namespace WrongSuspect.Events
{
    /// <summary>Runs the Inspector-configured response whenever its GameEvent asset is raised.</summary>
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField] private GameEvent gameEvent;
        [SerializeField] private UnityEvent response = new UnityEvent();

        private void OnEnable()
        {
            if (gameEvent != null)
            {
                gameEvent.Raised += OnRaised;
            }
        }

        private void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.Raised -= OnRaised;
            }
        }

        private void OnRaised() => response.Invoke();
    }
}
