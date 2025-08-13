using System.Collections.Generic;
using UnityEngine;
using GearSystem;

public class GearRotator : MonoBehaviour
{
    public static GearRotator Instance { get; private set; }

    [Header("Meshing / traversal")]
    [Tooltip("Tolerance (in world units) when checking mesh distance. Increase slightly if sprites aren't perfectly sized.")]
    [SerializeField] private float meshTolerance = 0.06f;

    private static readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Public API: rotate the connected *meshed* cluster starting from the grid position.
    /// Call this with the gear that entered the trigger's GridPosition.
    /// </summary>
    public void RotateConnectedGears(Vector2Int startPos)
    {
        GridManager grid = GridManager.Instance;
        if (grid == null) return;

        if (!grid.IsInsideGrid(startPos)) return;

        var visited = new HashSet<Vector2Int>();
        RotateRecursive(startPos, true, visited);
    }

    private void RotateRecursive(Vector2Int pos, bool clockwise, HashSet<Vector2Int> visited)
    {
        GridManager grid = GridManager.Instance;
        if (grid == null) return;
        if (!grid.IsInsideGrid(pos) || visited.Contains(pos)) return;

        GearBase gear = grid.GetGearAt(pos);
        if (gear == null) return;

        visited.Add(pos);

        // rotate visual
        gear.RotateOnce(clockwise);

        // only increase fill for character gears that are active (activation logic belongs to GearLinkResolver)
        if (gear is CharacterGear cg && cg.IsActive)
            cg.AdvanceProgress();

        // traverse 4 grid neighbours, but recurse only if they are present AND actually meshed
        foreach (var dir in directions)
        {
            Vector2Int nPos = pos + dir;
            if (!grid.IsInsideGrid(nPos) || visited.Contains(nPos)) continue;

            GearBase neighbor = grid.GetGearAt(nPos);
            if (neighbor == null) continue;

            if (AreGearsMeshed(gear, neighbor))
            {
                RotateRecursive(nPos, !clockwise, visited);
            }
        }
    }

    // Use circle collider radii (scaled by lossyScale) to decide if two gears are meshed
    private bool AreGearsMeshed(GearBase a, GearBase b)
    {
        if (a == null || b == null) return false;

        CircleCollider2D ca = a.GetComponent<CircleCollider2D>();
        CircleCollider2D cb = b.GetComponent<CircleCollider2D>();

        if (ca == null || cb == null)
        {
            float approxA = ApproxRadiusFromRenderer(a);
            float approxB = ApproxRadiusFromRenderer(b);
            float dist = Vector2.Distance(a.transform.position, b.transform.position);
            float expected = approxA + approxB;
            return Mathf.Abs(dist - expected) <= meshTolerance;
        }

        // collider-radius * lossyScale.x (assuming uniform scale)
        float ra = ca.radius * GetWorldScale(a.transform);
        float rb = cb.radius * GetWorldScale(b.transform);

        float distCenters = Vector2.Distance(a.transform.position, b.transform.position);
        float expectedDist = ra + rb;

        // If colliders are actually overlapping slightly due to rounding, allow small tolerance
        bool meshed = Mathf.Abs(distCenters - expectedDist) <= meshTolerance;

        return meshed;
    }

    private float GetWorldScale(Transform t)
    {
        // assume uniform scale; take x
        return t.lossyScale.x;
    }

    private float ApproxRadiusFromRenderer(GearBase g)
    {
        SpriteRenderer sr = g.GetComponent<SpriteRenderer>();
        if (sr == null) return 0.5f;
        // approximate as half of the largest dimension in world units
        Vector2 size = sr.bounds.size;
        return Mathf.Max(size.x, size.y) * 0.5f;
    }
}