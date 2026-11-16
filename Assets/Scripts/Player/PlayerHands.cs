using UnityEngine;
using WrongSuspect.Interactables;

namespace WrongSuspect.Player
{
    /// <summary>Holds the one item the culprit is carrying and announces every change.</summary>
    public class PlayerHands : MonoBehaviour
    {
        [Header("Raises")]
        [SerializeField] private CarriableEvent carriedChanged;

        public Carriable Carried { get; private set; }
        public bool IsEmpty => Carried == null;

        public void Take(Carriable item)
        {
            item.PickUp();
            Carried = item;
            Announce();
        }

        public Carriable Release()
        {
            Carriable item = Carried;
            Carried = null;
            Announce();
            return item;
        }

        private void Announce()
        {
            if (carriedChanged != null)
            {
                carriedChanged.Raise(Carried);
            }
        }
    }
}
