using UnityEngine;

namespace WrongSuspect.Interactables
{
    /// <summary>
    /// Evidence that can be picked up, carried, placed somewhere else or hidden in a container.
    /// Sherlock can later read where each item ended up.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class Carriable : MonoBehaviour
    {
        public enum State { InWorld, Carried, Hidden }

        public State Current { get; private set; } = State.InWorld;
        public Container HiddenIn { get; private set; }
        public Interactable PlacedAt { get; private set; }

        public Sprite Icon => GetComponent<SpriteRenderer>().sprite;
        public string DisplayName => GetComponent<Interactable>().DisplayName;

        public void PickUp()
        {
            Current = State.Carried;
            HiddenIn = null;
            PlacedAt = null;
            gameObject.SetActive(false);
        }

        public void PlaceAt(Vector2 point, Interactable spot)
        {
            transform.position = point;
            PlacedAt = spot;
            Current = State.InWorld;
            gameObject.SetActive(true);
        }

        public void HideIn(Container container)
        {
            HiddenIn = container;
            Current = State.Hidden;
            gameObject.SetActive(false);
        }
    }
}
