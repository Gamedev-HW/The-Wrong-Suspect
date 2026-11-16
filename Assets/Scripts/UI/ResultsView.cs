using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WrongSuspect.Investigation;

namespace WrongSuspect.UI
{
    /// <summary>
    /// The end screen: who Sherlock accused, a stamp and medal for the result, and the clues for and against you.
    /// Its buttons raise the restart / next-level event assets (wired in the Inspector).
    /// </summary>
    public class ResultsView : MonoBehaviour
    {
        [Header("Listens to")]
        [SerializeField] private VerdictEvent verdictReady;

        [Header("Parts")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Image portrait;
        [SerializeField] private Text headline;
        [SerializeField] private Text findings;
        [SerializeField] private Image stamp;
        [SerializeField] private Image medal;
        [SerializeField] private GameObject nextLevelButton;

        [Header("Full victory")]
        [SerializeField] private string fullVictoryText = "Sherlock accused {0}. You got away with it!";
        [SerializeField] private Sprite fullVictoryStamp;
        [SerializeField] private Sprite fullVictoryMedal;

        [Header("Partial victory")]
        [SerializeField] private string partialVictoryText = "Sherlock accused {0}... but he suspects the scene was arranged.";
        [SerializeField] private Sprite partialVictoryStamp;
        [SerializeField] private Sprite partialVictoryMedal;

        [Header("Failure")]
        [SerializeField] private string failureText = "Sherlock saw through you. You are arrested!";
        [SerializeField] private Sprite failureStamp;
        [SerializeField] private Sprite failureMedal;

        private void Awake()
        {
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (verdictReady != null) verdictReady.Raised += Show;
        }

        private void OnDisable()
        {
            if (verdictReady != null) verdictReady.Raised -= Show;
        }

        private void Show(Verdict verdict)
        {
            switch (verdict.Result)
            {
                case Verdict.Outcome.FullVictory:
                    Fill(verdict, fullVictoryText, fullVictoryStamp, fullVictoryMedal);
                    break;
                case Verdict.Outcome.PartialVictory:
                    Fill(verdict, partialVictoryText, partialVictoryStamp, partialVictoryMedal);
                    break;
                default:
                    Fill(verdict, failureText, failureStamp, failureMedal);
                    break;
            }
            nextLevelButton.SetActive(verdict.Result != Verdict.Outcome.Failure);
            panel.SetActive(true);
            stamp.gameObject.SetActive(false);      // re-enabling replays the stamp's slam animation
            stamp.gameObject.SetActive(true);
        }

        private void Fill(Verdict verdict, string text, Sprite stampSprite, Sprite medalSprite)
        {
            headline.text = string.Format(text, verdict.Accused.DisplayName);
            portrait.sprite = verdict.Accused.Portrait;
            stamp.sprite = stampSprite;
            medal.sprite = medalSprite;
            findings.text = DescribeClues(verdict);
        }

        /// <summary>"+" lines pointed away from you, "-" lines pointed at you or exposed the set-up.</summary>
        private static string DescribeClues(Verdict verdict)
        {
            var text = new StringBuilder();
            foreach (ClueRule clue in verdict.Clues)
            {
                bool againstYou = clue.IsContradiction || (clue.Suspect != null && clue.Suspect.IsCulprit && clue.Weight > 0);
                text.Append(againstYou ? "-  " : "+  ").AppendLine(clue.Line);
            }
            return text.ToString();
        }
    }
}
