using System.Collections.Generic;
using UnityEngine;
using GearSystem;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid Size")]
    [SerializeField] private int gridWidth = 6, gridHeight = 6;
    [SerializeField] private float cellSize = 0.75f;

    [Header("Visuals")]
    [SerializeField] private GameObject gridCellPrefab;
    [SerializeField] private Transform gridVisualParent;

    [Header("Gear Prefabs")]
    [SerializeField] private GameObject motorPrefab;

    private GearBase[,] grid;
    private HashSet<Vector2Int> activePathGears = new HashSet<Vector2Int>();

    public int Width => gridWidth;
    public int Height => gridHeight;
    public HashSet<Vector2Int> ActivePathGears => activePathGears;

    public delegate void ActivePathChangedHandler(HashSet<Vector2Int> activeGears);
    public event ActivePathChangedHandler OnActivePathChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        grid = new GearBase[gridWidth, gridHeight];
    }

    private void Start()
    {
        GenerateGrid();
        PlaceMotorAtRandomPosition();
    }

    #region Grid Setup
    private void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 pos = GridToWorld(new Vector2Int(x, y));
                Instantiate(gridCellPrefab, pos, Quaternion.identity, gridVisualParent);
            }
        }
    }

    private void PlaceMotorAtRandomPosition()
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (IsCellEmpty(pos))
                    emptyCells.Add(pos);
            }
        }

        if (emptyCells.Count == 0) return;

        Vector2Int randomPos = emptyCells[Random.Range(0, emptyCells.Count)];
        Vector3 worldPos = GridToWorld(randomPos);

        GameObject motorGO = Instantiate(motorPrefab, worldPos, Quaternion.identity);
        GearBase motorGear = motorGO.GetComponent<GearBase>();

        if (motorGear != null)
        {
            motorGear.GridPosition = randomPos;
            PlaceGear(motorGear, randomPos);
        }
    }
    #endregion

    #region Core Storage
    public bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }

    public bool IsCellEmpty(Vector2Int pos)
    {
        return IsInsideGrid(pos) && grid[pos.x, pos.y] == null;
    }

    public void PlaceGear(GearBase gear, Vector2Int pos)
    {
        if (!IsInsideGrid(pos)) return;
        grid[pos.x, pos.y] = gear;
        gear.GridPosition = pos;
    }

    public void RemoveGear(Vector2Int pos)
    {
        if (!IsInsideGrid(pos)) return;
        grid[pos.x, pos.y] = null;
    }

    public GearBase GetGearAt(Vector2Int pos)
    {
        return IsInsideGrid(pos) ? grid[pos.x, pos.y] : null;
    }

    public void SetActivePath(HashSet<Vector2Int> path)
    {
        activePathGears = new HashSet<Vector2Int>(path ?? new HashSet<Vector2Int>());
        OnActivePathChanged?.Invoke(activePathGears);
    }
    #endregion

    #region Grid Position Conversion
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        Vector3 offset = new Vector3(
            (gridPos.x - gridWidth / 2f + 0.5f) * cellSize,
            (gridPos.y - gridHeight / 2f + 0.5f) * cellSize,
            0f
        );
        return transform.position + offset;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - transform.position;
        int x = Mathf.FloorToInt(localPos.x / cellSize + gridWidth / 2f);
        int y = Mathf.FloorToInt(localPos.y / cellSize + gridHeight / 2f);
        return new Vector2Int(x, y);
    }
    #endregion

    public List<GearBase> GetAllGears()
    {
        List<GearBase> gears = new List<GearBase>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                    gears.Add(grid[x, y]);
            }
        }

        return gears;
    }

    public HashSet<Vector2Int> GetActivePath()
    {
        return activePathGears; // <-- whatever variable you store it in
    }

}
