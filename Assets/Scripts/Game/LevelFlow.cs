using UnityEngine;
using UnityEngine.SceneManagement;
using WrongSuspect.Events;

namespace WrongSuspect.Game
{
    /// <summary>
    /// Restarts the level or moves on to the next one. Loading the scene again resets everything
    /// (positions, carried items, clock, Sherlock), because all game state lives in the scene.
    /// The level scenes must be in File > Build Profiles > Scene List (the tool adds them).
    /// </summary>
    public class LevelFlow : MonoBehaviour
    {
        [SerializeField, Tooltip("How many level scenes come first in the build's scene list.")]
        private int levelCount = 4;

        [Header("Listens to")]
        [SerializeField] private GameEvent restartRequested;
        [SerializeField] private GameEvent nextLevelRequested;

        private void OnEnable()
        {
            if (restartRequested != null) restartRequested.Raised += Restart;
            if (nextLevelRequested != null) nextLevelRequested.Raised += NextLevel;
        }

        private void OnDisable()
        {
            if (restartRequested != null) restartRequested.Raised -= Restart;
            if (nextLevelRequested != null) nextLevelRequested.Raised -= NextLevel;
        }

        private void Restart() => Load(SceneManager.GetActiveScene().buildIndex);

        /// <summary>After the last level it goes back to the first.</summary>
        private void NextLevel()
        {
            int count = Mathf.Min(levelCount, SceneManager.sceneCountInBuildSettings);
            int current = SceneManager.GetActiveScene().buildIndex;
            Load(count <= 0 || current < 0 ? -1 : (current + 1) % count);
        }

        private static void Load(int buildIndex)
        {
            if (buildIndex < 0)
            {
                Debug.LogError("[Wrong Suspect] This scene is not in File > Build Profiles > Scene List, so it can't be reloaded.");
                return;
            }
            SceneManager.LoadScene(buildIndex);
        }
    }
}
