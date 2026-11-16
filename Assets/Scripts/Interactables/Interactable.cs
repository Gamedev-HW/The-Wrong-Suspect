using System.Collections.Generic;
using UnityEngine;
using WrongSuspect.Actions;

namespace WrongSuspect.Interactables
{
    /// <summary>
    /// An object the culprit can walk to and act on: its name, description, the actions it offers,
    /// and where he stands while using it. Clicking it raises the "selected" event asset.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 4)] private string description;
        [SerializeField] private GameAction[] actions = new GameAction[0];

        [Header("Positions (relative to the pivot)")]
        [SerializeField, Tooltip("Where the culprit stands to use this object.")]
        private Vector2 standOffset = new Vector2(0f, -0.7f);
        [SerializeField] private bool acceptsPlacedItems;
        [SerializeField, Tooltip("Where an item placed here ends up.")]
        private Vector2 dropOffset = new Vector2(0.6f, 0f);

        [Header("Raises")]
        [SerializeField] private InteractableEvent selected;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string Description => description;
        public bool AcceptsPlacedItems => acceptsPlacedItems;
        public Vector2 StandPoint => (Vector2)transform.position + standOffset;
        public Vector2 DropPoint => (Vector2)transform.position + dropOffset;

        /// <summary>Distance from a point to the object's edge (its click area).</summary>
        public float DistanceTo(Vector2 point)
        {
            var area = GetComponent<Collider2D>();
            Vector2 closest = area != null ? area.ClosestPoint(point) : (Vector2)transform.position;
            return Vector2.Distance(point, closest);
        }

        /// <summary>Called when the object is clicked (wired from PointerTarget in the prefab).</summary>
        public void Select()
        {
            if (selected != null)
            {
                selected.Raise(this);
            }
        }

        /// <summary>The actions that make sense right now, e.g. "Place here" only while carrying something.</summary>
        public List<GameAction> AvailableActions(ActionContext context)
        {
            var available = new List<GameAction>();
            foreach (GameAction action in actions)
            {
                if (action != null && action.IsAvailable(context))
                {
                    available.Add(action);
                }
            }
            return available;
        }
    }
}
