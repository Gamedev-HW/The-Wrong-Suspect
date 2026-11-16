using System.Collections;
using UnityEngine;
using WrongSuspect.Characters;
using WrongSuspect.Detective;
using WrongSuspect.Dialogue;
using WrongSuspect.Events;
using WrongSuspect.Interactables;
using WrongSuspect.Navigation;

namespace WrongSuspect.Investigation
{
    /// <summary>
    /// Sherlock's turn. When the preparation ends it asks for an alibi (if the level has one), then sends Sherlock in:
    /// he opens the doors, walks to each clue that holds, examines it and says what it means, thinks, and accuses.
    /// The verdict is announced through an event asset for the results screen.
    /// </summary>
    public class SherlockInvestigation : MonoBehaviour
    {
        [SerializeField] private CaseFile caseFile;

        [Header("Sherlock in the scene (starts switched off at the entrance)")]
        [SerializeField] private GameObject sherlock;
        [SerializeField, Tooltip("Doors Sherlock opens as he arrives, so he can reach every clue.")]
        private OpenableAnimator[] doors = new OpenableAnimator[0];
        [SerializeField] private PathMover sherlockMover;
        [SerializeField] private DetectiveAnimator sherlockPoses;
        [SerializeField] private CharacterAnimator sherlockBody;

        [Header("Dialogue")]
        [SerializeField] private string sherlockName = "Sherlock Holmes";
        [SerializeField] private Sprite portrait;
        [SerializeField] private Sprite thinkingPortrait;
        [SerializeField] private Sprite triumphantPortrait;
        [SerializeField, TextArea(1, 2)] private string openingLine = "Now then. Let us see what this room has to tell me.";
        [SerializeField, TextArea(1, 2)] private string thinkingLine = "Hmm... it all points one way.";
        [SerializeField, Tooltip("{0} = the accused.")] private string accusationLine = "It was {0}!";

        [Header("Pacing (seconds)")]
        [SerializeField] private float lineSeconds = 2.6f;
        [SerializeField] private float inspectSeconds = 1.2f;
        [SerializeField] private float thinkSeconds = 2f;

        [Header("Listens to")]
        [SerializeField] private GameEvent preparationEnded;
        [SerializeField] private IntEvent alibiChosen;

        [Header("Raises")]
        [SerializeField] private AlibiRequestEvent alibiRequested;
        [SerializeField] private DialogueEvent dialogue;
        [SerializeField] private VerdictEvent verdictReady;

        private bool started;

        private void OnEnable()
        {
            if (preparationEnded != null) preparationEnded.Raised += OnPreparationEnded;
            if (alibiChosen != null) alibiChosen.Raised += Begin;
        }

        private void OnDisable()
        {
            if (preparationEnded != null) preparationEnded.Raised -= OnPreparationEnded;
            if (alibiChosen != null) alibiChosen.Raised -= Begin;
        }

        private void OnPreparationEnded()
        {
            if (started)
            {
                return;
            }
            if (caseFile.HasAlibi)
            {
                alibiRequested.Raise(caseFile.AlibiOptions);
            }
            else
            {
                Begin(-1);
            }
        }

        private void Begin(int chosenAlibi)
        {
            if (started)
            {
                return;
            }
            started = true;
            StartCoroutine(Investigate(caseFile.Judge(chosenAlibi)));
        }

        private IEnumerator Investigate(Verdict verdict)
        {
            foreach (OpenableAnimator door in doors)
            {
                if (door != null)
                {
                    door.Open();
                }
            }
            sherlock.SetActive(true);
            yield return Say(portrait, openingLine);

            foreach (ClueRule clue in verdict.Clues)
            {
                if (clue.LookAt != null)
                {
                    yield return WalkTo(StandPointFor(clue.LookAt));
                    sherlockBody.FaceTowards(clue.LookAt);
                    sherlockPoses.StartInspecting();
                    yield return new WaitForSeconds(inspectSeconds);
                    sherlockPoses.StopInspecting();
                }
                yield return Say(portrait, clue.Line);
            }

            sherlockBody.SetFacing(Vector2.down);
            sherlockPoses.StartThinking();
            dialogue.Raise(new DialogueLine(sherlockName, thinkingPortrait, thinkingLine));
            yield return new WaitForSeconds(thinkSeconds);
            sherlockPoses.StopThinking();

            sherlockPoses.Accuse();
            yield return Say(triumphantPortrait, string.Format(accusationLine, verdict.Accused.DisplayName));

            dialogue.Raise(DialogueLine.Hide);
            verdictReady.Raise(verdict);
        }

        private IEnumerator Say(Sprite face, string text)
        {
            dialogue.Raise(new DialogueLine(sherlockName, face, text));
            yield return new WaitForSeconds(lineSeconds);
        }

        private IEnumerator WalkTo(Vector2 point)
        {
            bool arrived = false;
            if (sherlockMover.MoveTo(point, _ => arrived = true))
            {
                yield return new WaitUntil(() => arrived);
            }
        }

        private static Vector2 StandPointFor(Transform target) =>
            target.TryGetComponent(out Interactable interactable)
                ? interactable.StandPoint
                : (Vector2)target.position + new Vector2(0f, -0.7f);
    }
}
