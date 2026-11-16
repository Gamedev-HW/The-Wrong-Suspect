using UnityEngine;
using UnityEngine.UI;
using WrongSuspect.Dialogue;

namespace WrongSuspect.UI
{
    /// <summary>The dialogue box at the bottom of the screen: speaker portrait, name and line. Short lines hide by themselves.</summary>
    public class DialogueView : MonoBehaviour
    {
        [Header("Listens to")]
        [SerializeField] private DialogueEvent dialogue;

        [Header("Parts")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Image portrait;
        [SerializeField] private Text speaker;
        [SerializeField] private Text line;

        private float hideAt = -1f;

        private void Awake()
        {
            panel.SetActive(false);
        }

        private void Update()
        {
            if (hideAt > 0f && Time.time >= hideAt)
            {
                hideAt = -1f;
                panel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (dialogue != null) dialogue.Raised += Show;
        }

        private void OnDisable()
        {
            if (dialogue != null) dialogue.Raised -= Show;
        }

        private void Show(DialogueLine next)
        {
            bool visible = !string.IsNullOrEmpty(next.Text);
            panel.SetActive(visible);
            hideAt = visible && next.Seconds > 0f ? Time.time + next.Seconds : -1f;
            if (!visible)
            {
                return;
            }
            speaker.text = next.Speaker;
            line.text = next.Text;
            portrait.sprite = next.Portrait;
            portrait.enabled = next.Portrait != null;
        }
    }
}
