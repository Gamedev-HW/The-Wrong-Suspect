using System.Collections;
using UnityEngine;
using WrongSuspect.Actions;
using WrongSuspect.Characters;
using WrongSuspect.Dialogue;
using WrongSuspect.Events;
using WrongSuspect.Interactables;
using WrongSuspect.Navigation;

namespace WrongSuspect.Player
{
    /// <summary>
    /// The point-and-click loop: click the floor and the culprit walks there; click an object and he walks to it,
    /// the action menu is requested, and the chosen action is played and carried out.
    /// If a closed door is in the way he walks as close as he can and says so.
    /// It talks to the UI only through event assets.
    /// </summary>
    [RequireComponent(typeof(PathMover), typeof(PlayerHands), typeof(PlayerActionAnimator))]
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Listens to")]
        [SerializeField] private InteractableEvent interactableSelected;
        [SerializeField] private Vector2Event floorClicked;
        [SerializeField] private ActionEvent actionChosen;
        [SerializeField] private GameEvent preparationEnded;

        [Header("Raises")]
        [SerializeField] private ActionMenuEvent menuRequested;
        [SerializeField] private GameEvent menuClosed;
        [SerializeField] private FloatEvent timeSpent;
        [SerializeField] private DialogueEvent dialogue;

        [Header("When the way is blocked")]
        [SerializeField, Tooltip("He can use an object from this far away (world units), e.g. a door from the other side.")]
        private float reach = 1f;
        [SerializeField] private string speakerName = "You";
        [SerializeField] private Sprite portrait;
        [SerializeField] private string blockedLine = "The way is blocked. I'll have to open the door first.";
        [SerializeField] private float blockedLineSeconds = 2.5f;

        private PathMover mover;
        private PlayerHands hands;
        private PlayerActionAnimator actionAnimator;
        private CharacterAnimator body;
        private Interactable target;
        private bool busy;
        private bool finished;

        private void Awake()
        {
            mover = GetComponent<PathMover>();
            hands = GetComponent<PlayerHands>();
            actionAnimator = GetComponent<PlayerActionAnimator>();
            body = GetComponent<CharacterAnimator>();
        }

        private void OnEnable()
        {
            if (interactableSelected != null) interactableSelected.Raised += OnInteractableSelected;
            if (floorClicked != null) floorClicked.Raised += OnFloorClicked;
            if (actionChosen != null) actionChosen.Raised += OnActionChosen;
            if (preparationEnded != null) preparationEnded.Raised += OnPreparationEnded;
        }

        private void OnDisable()
        {
            if (interactableSelected != null) interactableSelected.Raised -= OnInteractableSelected;
            if (floorClicked != null) floorClicked.Raised -= OnFloorClicked;
            if (actionChosen != null) actionChosen.Raised -= OnActionChosen;
            if (preparationEnded != null) preparationEnded.Raised -= OnPreparationEnded;
        }

        private bool CanAct => !busy && !finished;

        private void OnFloorClicked(Vector2 point)
        {
            if (!CanAct)
            {
                return;
            }
            target = null;
            menuClosed.Raise();
            mover.MoveTo(point, OnFloorReached);
        }

        private void OnFloorReached(bool reached)
        {
            if (!reached)
            {
                SayBlocked();
            }
        }

        private void OnInteractableSelected(Interactable selected)
        {
            if (!CanAct)
            {
                return;
            }
            target = selected;
            menuClosed.Raise();
            mover.MoveTo(selected.StandPoint, OnArrived);
        }

        private void OnArrived(bool reached)
        {
            if (target == null)
            {
                return;
            }
            if (!reached && target.DistanceTo(transform.position) > reach)
            {
                SayBlocked();
                return;
            }
            if (body != null)
            {
                body.FaceTowards(target.transform);
            }
            var context = new ActionContext(target, hands);
            menuRequested.Raise(new ActionMenuRequest(target, target.AvailableActions(context), context));
        }

        private void OnActionChosen(GameAction action)
        {
            if (!CanAct || target == null || action == null)
            {
                return;
            }
            StartCoroutine(Perform(action, new ActionContext(target, hands)));
        }

        private IEnumerator Perform(GameAction action, ActionContext context)
        {
            busy = true;
            if (action.UsesBusyAnimation)
            {
                actionAnimator.StartBusy();
            }
            else
            {
                actionAnimator.PlayInteract();
            }

            yield return new WaitForSeconds(action.AnimationSeconds);

            actionAnimator.StopBusy();
            if (!finished && action.IsAvailable(context))
            {
                action.Execute(context);
                timeSpent.Raise(action.TimeCost);
            }
            busy = false;
        }

        private void SayBlocked()
        {
            if (dialogue != null)
            {
                dialogue.Raise(new DialogueLine(speakerName, portrait, blockedLine, blockedLineSeconds));
            }
        }

        private void OnPreparationEnded()
        {
            finished = true;
            mover.Stop();
            menuClosed.Raise();
        }
    }
}
