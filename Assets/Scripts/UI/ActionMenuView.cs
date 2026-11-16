using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WrongSuspect.Actions;
using WrongSuspect.Events;

namespace WrongSuspect.UI
{
    /// <summary>
    /// The pop-up menu next to the clicked object. It shows the actions it is given and reports the chosen one
    /// through an event asset; it never calls game code directly.
    /// </summary>
    public class ActionMenuView : MonoBehaviour
    {
        [Header("Listens to")]
        [SerializeField] private ActionMenuEvent menuRequested;
        [SerializeField] private GameEvent menuClosed;

        [Header("Raises")]
        [SerializeField] private ActionEvent actionChosen;

        [Header("Parts")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private Text title;
        [SerializeField] private ActionButtonView buttonTemplate;
        [SerializeField] private Button cancelButton;
        [SerializeField, Tooltip("Screen pixels between the object and the bottom of the menu.")]
        private Vector2 screenOffset = new Vector2(0f, 70f);

        private readonly List<ActionButtonView> buttons = new List<ActionButtonView>();

        private void Awake()
        {
            buttonTemplate.gameObject.SetActive(false);
            panel.gameObject.SetActive(false);
            cancelButton.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            if (menuRequested != null) menuRequested.Raised += Show;
            if (menuClosed != null) menuClosed.Raised += Close;
        }

        private void OnDisable()
        {
            if (menuRequested != null) menuRequested.Raised -= Show;
            if (menuClosed != null) menuClosed.Raised -= Close;
        }

        private void Show(ActionMenuRequest request)
        {
            ClearButtons();
            title.text = request.Target.DisplayName;
            foreach (GameAction action in request.Actions)
            {
                ActionButtonView button = Instantiate(buttonTemplate, buttonTemplate.transform.parent);
                button.gameObject.SetActive(true);
                button.Bind(action.LabelFor(request.Context), action.Icon, action.TimeCost, () => Choose(action));
                buttons.Add(button);
            }
            cancelButton.transform.SetAsLastSibling();
            panel.gameObject.SetActive(true);
            PlaceNear(request.Target.transform.position);
        }

        private void PlaceNear(Vector3 worldPosition)
        {
            Camera cam = Camera.main;
            Vector2 screen = cam != null ? (Vector2)cam.WorldToScreenPoint(worldPosition) : new Vector2(Screen.width, Screen.height) * 0.5f;
            screen += screenOffset;

            // keep the whole menu on screen (pivot is bottom-centre)
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
            Vector2 size = panel.rect.size * panel.lossyScale.x;
            screen.x = Mathf.Clamp(screen.x, size.x * 0.5f, Screen.width - size.x * 0.5f);
            screen.y = Mathf.Clamp(screen.y, 0f, Screen.height - size.y);
            panel.position = screen;
        }

        private void Choose(GameAction action)
        {
            Close();
            actionChosen.Raise(action);
        }

        private void Close()
        {
            ClearButtons();
            panel.gameObject.SetActive(false);
        }

        private void ClearButtons()
        {
            foreach (ActionButtonView button in buttons)
            {
                Destroy(button.gameObject);
            }
            buttons.Clear();
        }
    }
}
