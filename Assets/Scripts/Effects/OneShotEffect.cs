using UnityEngine;

namespace WrongSuspect.Effects
{
    /// <summary>
    /// Hides (or destroys) a one-shot effect once its animation is over.
    /// The Animator replays the clip every time the object is enabled.
    /// </summary>
    public class OneShotEffect : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.5f;
        [SerializeField, Tooltip("Destroy instead of disabling (for effects spawned with Instantiate).")]
        private bool destroyWhenDone;

        private float timeLeft;

        private void OnEnable()
        {
            timeLeft = lifetime;
        }

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft > 0f)
            {
                return;
            }

            if (destroyWhenDone)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
