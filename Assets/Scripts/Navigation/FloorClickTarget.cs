using UnityEngine;
using UnityEngine.EventSystems;
using WrongSuspect.Events;

namespace WrongSuspect.Navigation
{
    /// <summary>
    /// Sits on the room and reports where on the floor the player clicked.
    /// Objects in the room are drawn above it, so their own clicks win.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FloorClickTarget : MonoBehaviour, IPointerClickHandler
    {
        [Header("Raises")]
        [SerializeField] private Vector2Event floorClicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && floorClicked != null)
            {
                floorClicked.Raise(eventData.pointerCurrentRaycast.worldPosition);
            }
        }
    }
}
