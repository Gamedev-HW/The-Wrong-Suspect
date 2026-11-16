using UnityEngine;
using UnityEngine.UI;
using WrongSuspect.Interactables;

namespace WrongSuspect.UI
{
    /// <summary>Shows what the culprit is carrying (icon and name), or nothing when his hands are empty.</summary>
    public class CarriedItemView : MonoBehaviour
    {
        [Header("Listens to")]
        [SerializeField] private CarriableEvent carriedChanged;

        [Header("Parts")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Image icon;
        [SerializeField] private Text itemName;

        private void Awake()
        {
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (carriedChanged != null) carriedChanged.Raised += Show;
        }

        private void OnDisable()
        {
            if (carriedChanged != null) carriedChanged.Raised -= Show;
        }

        private void Show(Carriable item)
        {
            panel.SetActive(item != null);
            if (item != null)
            {
                icon.sprite = item.Icon;
                itemName.text = item.DisplayName;
            }
        }
    }
}
