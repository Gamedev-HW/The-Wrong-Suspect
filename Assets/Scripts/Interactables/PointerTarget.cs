using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace WrongSuspect.Interactables
{
    /// <summary>
    /// Turns mouse hover and clicks on this object's collider into UnityEvents.
    /// In the prefabs: hover shows/hides the highlight and a click selects the Interactable,
    /// all wired in the Inspector. Needs an EventSystem and a Physics 2D Raycaster on the camera.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PointerTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private UnityEvent entered = new UnityEvent();
        [SerializeField] private UnityEvent exited = new UnityEvent();
        [SerializeField] private UnityEvent clicked = new UnityEvent();

        public UnityEvent Entered => entered;
        public UnityEvent Exited => exited;
        public UnityEvent Clicked => clicked;

        public void OnPointerEnter(PointerEventData eventData) => entered.Invoke();

        public void OnPointerExit(PointerEventData eventData) => exited.Invoke();

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                clicked.Invoke();
            }
        }
    }
}
