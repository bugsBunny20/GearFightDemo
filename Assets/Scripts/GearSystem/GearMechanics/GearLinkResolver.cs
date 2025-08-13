using System.Collections.Generic;
using UnityEngine;
using GearSystem;

public class GearLinkResolver : MonoBehaviour
{
    [SerializeField] private bool debugLogs = false;

    private static readonly Vector2Int[] directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private void Start()
    {
        if (GearEventManager.Instance != null)
            GearEventManager.Instance.OnGearGridChanged += ResolveLinks;
        else
            Debug.LogWarning("[GearLinkResolver] GearEventManager.Instance is null in Start()");
    }

    private void OnDisable()
    {
        if (GearEventManager.Instance != null)
            GearEventManager.Instance.OnGearGridChanged -= ResolveLinks;
    }

    private void ResolveLinks()
    {
        GridManager grid = GridManager.Instance;
        if (grid == null)
        {
            Debug.LogWarning("[GearLinkResolver] GridManager.Instance is null");
            return;
        }

        // collect motor and character positions
        var motorPositions = new List<Vector2Int>();
        var charPositions = new List<Vector2Int>();

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                var pos = new Vector2Int(x, y);
                var g = grid.GetGearAt(pos);
                if (g == null) continue;
                if (g.gearType == GearType.Motor) motorPositions.Add(pos);
                else if (g.gearType == GearType.Character) charPositions.Add(pos);
            }
        }

        // nothing to do if either side missing
        if (motorPositions.Count == 0 || charPositions.Count == 0)
        {
            // ensure everything is inactive + store empty path
            var empty = new HashSet<Vector2Int>();
            ApplyActiveSet(grid, empty);
            grid.SetActivePath(empty);
            return;
        }

        //lood from all motors (no avoidance) -> nodes that can reach a motor
        var motorConnected = FloodFromStarts(grid, motorPositions, avoidMotorNodes: false);

        //flood from all characters but DO NOT step into motor nodes
        //nodes that can reach a character without passing through any motor
        var charConnectedAvoidingMotors = FloodFromStarts(grid, charPositions, avoidMotorNodes: true);

        //final = intersection of the two sets
        var finalActive = new HashSet<Vector2Int>(motorConnected);
        finalActive.IntersectWith(charConnectedAvoidingMotors);

        //include motors that neighbor any node in charConnectedAvoidingMotors
        //(this makes motors active when adjacent to a chain that reaches character
        //without going *through* another motor)
        foreach (var mpos in motorPositions)
        {
            foreach (var d in directions)
            {
                var n = mpos + d;
                if (!grid.IsInsideGrid(n)) continue;
                if (charConnectedAvoidingMotors.Contains(n))
                {
                    finalActive.Add(mpos);
                    break;
                }
            }
        }

        if (debugLogs)
        {
            Debug.Log("[GearLinkResolver] motorConnected: " + string.Join(", ", motorConnected));
            Debug.Log("[GearLinkResolver] charConnectedAvoidingMotors: " + string.Join(", ", charConnectedAvoidingMotors));
            Debug.Log("[GearLinkResolver] finalActive: " + string.Join(", ", finalActive));
        }

        // apply final active set and notify other systems by storing path in grid
        ApplyActiveSet(grid, finalActive);
        grid.SetActivePath(finalActive);
    }

    // BFS starting from multiple starts. If avoidMotorNodes==true, BFS will not traverse INTO motor nodes.
    private HashSet<Vector2Int> FloodFromStarts(GridManager grid, IEnumerable<Vector2Int> starts, bool avoidMotorNodes)
    {
        var visited = new HashSet<Vector2Int>();
        var q = new Queue<Vector2Int>();

        foreach (var s in starts)
        {
            visited.Add(s);
            q.Enqueue(s);
        }

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var d in directions)
            {
                var nxt = cur + d;
                if (!grid.IsInsideGrid(nxt)) continue;
                if (visited.Contains(nxt)) continue;

                var ng = grid.GetGearAt(nxt);
                if (ng == null) continue;

                if (avoidMotorNodes && ng.gearType == GearType.Motor)
                    continue; // explicitly skip entering motor nodes when avoiding

                visited.Add(nxt);
                q.Enqueue(nxt);
            }
        }

        return visited;
    }

    private void ApplyActiveSet(GridManager grid, HashSet<Vector2Int> activeSet)
    {
        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                var pos = new Vector2Int(x, y);
                var g = grid.GetGearAt(pos);
                if (g == null) continue;

                bool shouldBeActive = activeSet.Contains(pos);

                // Always call SetActive so visuals update,
                // even if the state boolean is already correct.
                g.SetActive(shouldBeActive);
            }
        }
    }
    private Vector2Int? FindGearOfType(GearType type)
    {
        GridManager gridManager = GridManager.Instance;
        if (gridManager == null) return null;

        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                GearBase gear = gridManager.GetGearAt(new Vector2Int(x, y));
                if (gear != null && gear.gearType == type)
                    return new Vector2Int(x, y);
            }
        }
        return null;
    }

    public List<GearBase> GetChainFromMotorToCharacter(CharacterGear character)
    {
        List<GearBase> chain = new List<GearBase>();
        GridManager grid = GridManager.Instance;
        if (grid == null) return chain;

        Vector2Int? motorPos = FindGearOfType(GearType.Motor);
        if (motorPos == null) return chain;

        // BFS to find shortest path from Motor to this CharacterGear
        Dictionary<Vector2Int, Vector2Int?> prev = new Dictionary<Vector2Int, Vector2Int?>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(motorPos.Value);
        prev[motorPos.Value] = null;

        Vector2Int targetPos = character.GridPosition;
        bool found = false;

        while (queue.Count > 0 && !found)
        {
            Vector2Int current = queue.Dequeue();
            if (current == targetPos)
            {
                found = true;
                break;
            }

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                if (!grid.IsInsideGrid(next)) continue;
                if (prev.ContainsKey(next)) continue;

                GearBase neighbor = grid.GetGearAt(next);
                if (neighbor == null) continue;

                prev[next] = current;
                queue.Enqueue(next);
            }
        }

        // If path found, reconstruct chain
        if (found)
        {
            Vector2Int? node = targetPos;
            while (node != null)
            {
                GearBase g = grid.GetGearAt(node.Value);
                if (g != null) chain.Insert(0, g);
                node = prev[node.Value];
            }
        }

        return chain;
    }
}