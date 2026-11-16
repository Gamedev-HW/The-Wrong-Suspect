using UnityEngine;
using UnityEngine.UI;
using WrongSuspect.Events;
using WrongSuspect.Interactables;

namespace WrongSuspect.UI
{
    /// <summary>Shows the name and description of an inspected object until the player closes it.</summary>
    public class InspectionView : MonoBehaviour
    {
        [Header("Listens to")]
        [SerializeField] private InteractableEvent inspected;
        [SerializeField] private GameEvent menuClosed;

        [Header("Parts")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Text title;
        [SerializeField] private Text body;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            panel.SetActive(false);
            closeButton.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            if (inspected != null) inspected.Raised += Show;
            if (menuClosed != null) menuClosed.Raised += Close;
        }

        private void OnDisable()
        {
            if (inspected != null) inspected.Raised -= Show;
            if (menuClosed != null) menuClosed.Raised -= Close;
        }

        private void Show(Interactable item)
        {
            title.text = item.DisplayName;
            body.text = item.Description;
            panel.SetActive(true);
        }

        private void Close() => panel.SetActive(false);
    }
}
