using GearSystem;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GridCell : MonoBehaviour
{
    private Vector2Int gridPosition;
    public void SetCoordinates(Vector2Int pos)
    {
        gridPosition = pos;
    }

    public bool IsOccupied { get; private set; }
    public GearDragWorld OccupyingGear { get; private set; }

    public void PlaceGear(GearDragWorld gear)
    {
        if (gear == null) return;

        if (OccupyingGear != null && OccupyingGear != gear)
        {
            ClearGear(); // clear existing gear in this cell
        }

        OccupyingGear = gear;
        IsOccupied = true;
        gear.transform.position = transform.position;
    }

    public void ClearGear()
    {
        OccupyingGear = null;
        IsOccupied = false;
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }
}