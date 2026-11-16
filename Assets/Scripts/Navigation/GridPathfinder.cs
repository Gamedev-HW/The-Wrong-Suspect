using System.Collections.Generic;
using UnityEngine;

namespace WrongSuspect.Navigation
{
    /// <summary>Complete = the path reaches the point; Partial = it stops as close as possible (e.g. at a closed door).</summary>
    public enum PathResult { NoPath, Complete, Partial }

    /// <summary>
    /// A* search on a NavGrid, followed by smoothing so characters walk in straight lines between corners.
    /// When the point can't be reached it returns the path to the closest reachable spot.
    /// Static because it keeps no state between searches.
    /// </summary>
    public static class GridPathfinder
    {
        private const float DiagonalCost = 1.41421356f;

        private static readonly Vector2Int[] Steps =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1),
        };

        public static PathResult FindPath(NavGrid grid, Vector2 from, Vector2 to, List<Vector2> path)
        {
            path.Clear();
            if (!TryNearestWalkable(grid, grid.CellAt(from), out Vector2Int start) ||
                !TryNearestWalkable(grid, grid.CellAt(to), out Vector2Int goal))
            {
                return PathResult.NoPath;
            }

            List<Vector2Int> cells = Search(grid, start, goal, out bool reachedGoal);
            List<Vector2Int> corners = Smooth(grid, cells);
            for (int i = 1; i < corners.Count; i++)
            {
                path.Add(grid.CellCenter(corners[i]));
            }

            // end exactly on the requested point when it is walkable, otherwise on the closest free cell
            Vector2 end = !reachedGoal ? grid.CellCenter(cells[cells.Count - 1])
                : goal == grid.CellAt(to) ? to : grid.CellCenter(goal);
            if (path.Count > 0)
            {
                path[path.Count - 1] = end;
            }
            else
            {
                path.Add(end);
            }
            return reachedGoal ? PathResult.Complete : PathResult.Partial;
        }

        /// <summary>A*; if the goal can't be reached, the path leads to the explored cell closest to it.</summary>
        private static List<Vector2Int> Search(NavGrid grid, Vector2Int start, Vector2Int goal, out bool reachedGoal)
        {
            int width = grid.Width;
            int count = width * grid.Height;
            var cost = new float[count];
            var parent = new int[count];
            var closed = new bool[count];
            for (int i = 0; i < count; i++)
            {
                cost[i] = float.MaxValue;
                parent[i] = -1;
            }

            int startIndex = start.y * width + start.x;
            int goalIndex = goal.y * width + goal.x;
            var open = new MinHeap();
            cost[startIndex] = 0f;
            open.Push(startIndex, Heuristic(start, goal));
            int closestIndex = startIndex;
            float closestDistance = (start - goal).sqrMagnitude;

            while (open.Count > 0)
            {
                int current = open.Pop();
                if (closed[current])
                {
                    continue;
                }
                if (current == goalIndex)
                {
                    reachedGoal = true;
                    return Rebuild(parent, current, width);
                }
                closed[current] = true;

                var cell = new Vector2Int(current % width, current / width);
                float distance = (cell - goal).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = current;
                }
                foreach (Vector2Int step in Steps)
                {
                    Vector2Int next = cell + step;
                    bool diagonal = step.x != 0 && step.y != 0;
                    if (!grid.IsWalkable(next) ||
                        (diagonal && (!grid.IsWalkable(new Vector2Int(next.x, cell.y)) || !grid.IsWalkable(new Vector2Int(cell.x, next.y)))))
                    {
                        continue;   // blocked, or would cut a corner
                    }

                    int nextIndex = next.y * width + next.x;
                    float newCost = cost[current] + (diagonal ? DiagonalCost : 1f);
                    if (!closed[nextIndex] && newCost < cost[nextIndex])
                    {
                        cost[nextIndex] = newCost;
                        parent[nextIndex] = current;
                        open.Push(nextIndex, newCost + Heuristic(next, goal));
                    }
                }
            }
            reachedGoal = false;
            return Rebuild(parent, closestIndex, width);
        }

        private static float Heuristic(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return dx + dy + (DiagonalCost - 2f) * Mathf.Min(dx, dy);
        }

        private static List<Vector2Int> Rebuild(int[] parent, int index, int width)
        {
            var cells = new List<Vector2Int>();
            for (int i = index; i != -1; i = parent[i])
            {
                cells.Add(new Vector2Int(i % width, i / width));
            }
            cells.Reverse();
            return cells;
        }

        /// <summary>Keeps only the cells where the direction has to change (string pulling).</summary>
        private static List<Vector2Int> Smooth(NavGrid grid, List<Vector2Int> cells)
        {
            var corners = new List<Vector2Int> { cells[0] };
            int anchor = 0;
            for (int i = 2; i < cells.Count; i++)
            {
                if (!LineIsClear(grid, cells[anchor], cells[i]))
                {
                    anchor = i - 1;
                    corners.Add(cells[anchor]);
                }
            }
            if (cells.Count > 1)
            {
                corners.Add(cells[cells.Count - 1]);
            }
            return corners;
        }

        private static bool LineIsClear(NavGrid grid, Vector2Int a, Vector2Int b)
        {
            Vector2 from = a + new Vector2(0.5f, 0.5f);
            Vector2 to = b + new Vector2(0.5f, 0.5f);
            int samples = Mathf.CeilToInt(Vector2.Distance(from, to) * 4f);
            for (int i = 0; i <= samples; i++)
            {
                Vector2 p = Vector2.Lerp(from, to, samples == 0 ? 0f : (float)i / samples);
                if (!grid.IsWalkable(new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y))))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool TryNearestWalkable(NavGrid grid, Vector2Int cell, out Vector2Int found)
        {
            cell = new Vector2Int(Mathf.Clamp(cell.x, 0, grid.Width - 1), Mathf.Clamp(cell.y, 0, grid.Height - 1));
            int maxRadius = Mathf.Max(grid.Width, grid.Height);
            for (int r = 0; r <= maxRadius; r++)
            {
                bool any = false;
                found = cell;
                float best = float.MaxValue;
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != r)
                        {
                            continue;   // only the ring at distance r
                        }
                        var candidate = new Vector2Int(cell.x + dx, cell.y + dy);
                        float distance = dx * dx + dy * dy;
                        if (distance < best && grid.IsWalkable(candidate))
                        {
                            best = distance;
                            found = candidate;
                            any = true;
                        }
                    }
                }
                if (any)
                {
                    return true;
                }
            }
            found = cell;
            return false;
        }

        /// <summary>Binary heap of cell indices ordered by priority (lowest first).</summary>
        private sealed class MinHeap
        {
            private readonly List<int> items = new List<int>();
            private readonly List<float> keys = new List<float>();

            public int Count => items.Count;

            public void Push(int item, float key)
            {
                items.Add(item);
                keys.Add(key);
                for (int i = items.Count - 1; i > 0;)
                {
                    int up = (i - 1) / 2;
                    if (keys[up] <= keys[i])
                    {
                        break;
                    }
                    Swap(i, up);
                    i = up;
                }
            }

            public int Pop()
            {
                int top = items[0];
                int last = items.Count - 1;
                items[0] = items[last];
                keys[0] = keys[last];
                items.RemoveAt(last);
                keys.RemoveAt(last);
                for (int i = 0; ;)
                {
                    int left = 2 * i + 1, right = left + 1, smallest = i;
                    if (left < items.Count && keys[left] < keys[smallest]) smallest = left;
                    if (right < items.Count && keys[right] < keys[smallest]) smallest = right;
                    if (smallest == i)
                    {
                        break;
                    }
                    Swap(i, smallest);
                    i = smallest;
                }
                return top;
            }

            private void Swap(int a, int b)
            {
                (items[a], items[b]) = (items[b], items[a]);
                (keys[a], keys[b]) = (keys[b], keys[a]);
            }
        }
    }
}
