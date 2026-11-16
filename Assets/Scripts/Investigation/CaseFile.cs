using System.Collections.Generic;
using UnityEngine;

namespace WrongSuspect.Investigation
{
    /// <summary>
    /// The level's case: the suspects, the alibi choices and Sherlock's clue rules.
    /// Judging adds up suspicion per suspect; the highest is accused (a tie goes against the culprit).
    /// </summary>
    public class CaseFile : MonoBehaviour
    {
        [SerializeField] private SuspectProfile culprit;
        [SerializeField] private SuspectProfile[] suspects = new SuspectProfile[0];
        [SerializeField, Tooltip("Suspicion the culprit starts with: he was in the house.")]
        private int culpritStartingSuspicion = 1;
        [SerializeField, TextArea(1, 2)] private string[] alibiOptions = new string[0];
        [SerializeField] private ClueRule[] clues = new ClueRule[0];

        public bool HasAlibi => alibiOptions.Length > 0;
        public string[] AlibiOptions => alibiOptions;

        public Verdict Judge(int chosenAlibi)
        {
            var scores = new Dictionary<SuspectProfile, int>();
            foreach (SuspectProfile s in suspects)
            {
                if (s != null)
                {
                    scores[s] = 0;
                }
            }
            scores[culprit] = culpritStartingSuspicion;

            var trueClues = new List<ClueRule>();
            bool arranged = false;
            foreach (ClueRule clue in clues)
            {
                if (!clue.IsTrue(chosenAlibi))
                {
                    continue;
                }
                trueClues.Add(clue);
                arranged |= clue.IsContradiction;
                if (clue.Suspect != null)
                {
                    scores.TryGetValue(clue.Suspect, out int current);
                    scores[clue.Suspect] = current + clue.Weight;
                }
            }

            SuspectProfile accused = culprit;
            foreach (KeyValuePair<SuspectProfile, int> pair in scores)
            {
                if (pair.Value > scores[accused])
                {
                    accused = pair.Key;
                }
            }

            Verdict.Outcome result = accused == culprit ? Verdict.Outcome.Failure
                : arranged ? Verdict.Outcome.PartialVictory : Verdict.Outcome.FullVictory;
            return new Verdict(accused, result, trueClues);
        }
    }
}
