using UnityEngine;
using WrongSuspect.Events;

namespace WrongSuspect.Game
{
    /// <summary>
    /// The preparation clock. It only runs while the culprit walks, and each action takes its own cost off.
    /// It ends the preparation when time runs out; the "End preparation" button raises the same event.
    /// </summary>
    public class PreparationTimer : MonoBehaviour
    {
        [SerializeField] private float totalSeconds = 300f;

        [Header("Listens to")]
        [SerializeField] private BoolEvent playerWalkingChanged;
        [SerializeField] private FloatEvent timeSpent;

        [Header("Raises (and listens to)")]
        [SerializeField] private TimerEvent timerChanged;
        [SerializeField] private GameEvent preparationEnded;

        private float remaining;
        private bool walking;
        private bool ended;

        public float RemainingSeconds => remaining;

        private void Awake()
        {
            remaining = totalSeconds;
        }

        private void OnEnable()
        {
            if (playerWalkingChanged != null) playerWalkingChanged.Raised += OnWalkingChanged;
            if (timeSpent != null) timeSpent.Raised += Spend;
            if (preparationEnded != null) preparationEnded.Raised += OnPreparationEnded;
        }

        private void OnDisable()
        {
            if (playerWalkingChanged != null) playerWalkingChanged.Raised -= OnWalkingChanged;
            if (timeSpent != null) timeSpent.Raised -= Spend;
            if (preparationEnded != null) preparationEnded.Raised -= OnPreparationEnded;
        }

        private void Start()
        {
            Announce();
        }

        private void Update()
        {
            if (walking)
            {
                Spend(Time.deltaTime);
            }
        }

        private void OnWalkingChanged(bool isWalking) => walking = isWalking;

        private void Spend(float seconds)
        {
            if (ended)
            {
                return;
            }
            remaining = Mathf.Max(0f, remaining - seconds);
            Announce();
            if (remaining <= 0f && preparationEnded != null)
            {
                preparationEnded.Raise();
            }
        }

        private void OnPreparationEnded()
        {
            ended = true;
            walking = false;
        }

        private void Announce()
        {
            if (timerChanged != null)
            {
                timerChanged.Raise(remaining, totalSeconds);
            }
        }
    }
}
