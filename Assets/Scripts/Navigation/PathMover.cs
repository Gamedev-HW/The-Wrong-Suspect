using System;
using System.Collections.Generic;
using UnityEngine;
using WrongSuspect.Events;

namespace WrongSuspect.Navigation
{
    /// <summary>
    /// Walks a character to a point along a path around walls and furniture, then calls back on arrival.
    /// Used by both the culprit and Sherlock. Can announce when walking starts and stops
    /// (the preparation clock only runs while the culprit walks).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PathMover : MonoBehaviour
    {
        [SerializeField] private float speed = 2.5f;
        [SerializeField, Tooltip("The room's walkable grid. Without it the character walks in a straight line.")]
        private NavGrid navGrid;

        [Header("Raises (optional)")]
        [SerializeField] private BoolEvent walkingChanged;

        private readonly List<Vector2> path = new List<Vector2>();
        private Rigidbody2D body;
        private int nextWaypoint;
        private Action<bool> onArrived;
        private bool reachesTarget;
        private bool walking;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        public bool IsWalking => walking;

        /// <summary>
        /// Starts walking. On arrival <paramref name="arrived"/> gets true if the point was reached,
        /// false if the walk stopped as close as possible (e.g. at a closed door).
        /// Returns false (and stays put) when there is nowhere to walk.
        /// </summary>
        public bool MoveTo(Vector2 point, Action<bool> arrived)
        {
            path.Clear();
            PathResult result = PathResult.Complete;
            if (navGrid == null)
            {
                path.Add(point);
            }
            else
            {
                result = navGrid.FindPath(body.position, point, path);
            }
            if (result == PathResult.NoPath)
            {
                Stop();
                return false;
            }

            reachesTarget = result == PathResult.Complete;
            nextWaypoint = 0;
            onArrived = arrived;
            SetWalking(true);
            return true;
        }

        public void Stop()
        {
            onArrived = null;
            SetWalking(false);
        }

        private void FixedUpdate()
        {
            if (!walking)
            {
                return;
            }

            // move up to one step along the path, turning at waypoints
            float step = speed * Time.fixedDeltaTime;
            Vector2 position = body.position;
            while (step > 0f && nextWaypoint < path.Count)
            {
                Vector2 toWaypoint = path[nextWaypoint] - position;
                float distance = toWaypoint.magnitude;
                if (distance > step)
                {
                    position += toWaypoint / distance * step;
                    step = 0f;
                }
                else
                {
                    position = path[nextWaypoint];
                    step -= distance;
                    nextWaypoint++;
                }
            }
            body.MovePosition(position);

            if (nextWaypoint >= path.Count)
            {
                Action<bool> arrived = onArrived;
                Stop();
                arrived?.Invoke(reachesTarget);
            }
        }

        private void SetWalking(bool value)
        {
            if (walking == value)
            {
                return;
            }
            walking = value;
            if (walkingChanged != null)
            {
                walkingChanged.Raise(walking);
            }
        }
    }
}
