using System.Collections.Generic;

namespace WrongSuspect.Investigation
{
    /// <summary>Sherlock's conclusion: who he accuses, how well the deception worked, and the clues behind it.</summary>
    public class Verdict
    {
        public enum Outcome { FullVictory, PartialVictory, Failure }

        public Verdict(SuspectProfile accused, Outcome result, IReadOnlyList<ClueRule> clues)
        {
            Accused = accused;
            Result = result;
            Clues = clues;
        }

        public SuspectProfile Accused { get; }
        public Outcome Result { get; }

        /// <summary>The clues that were true, in the order Sherlock examines them.</summary>
        public IReadOnlyList<ClueRule> Clues { get; }
    }
}
