using UnityEngine;
using GearSystem;

public class GearPlacementHandler : MonoBehaviour
{
    public static GearPlacementHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool TryPlaceDraggedGear(GearDragWorld gear, Vector3 originalWorldPos)
    {
        GridManager gridManager = GridManager.Instance;
        Vector2Int gridPos = gridManager.WorldToGrid(gear.transform.position);

        //Outside grid → cancel placement
        if (!gridManager.IsInsideGrid(gridPos))
        {
            gear.transform.position = originalWorldPos;
            return false;
        }

        GearBase droppedGear = gear.GetComponent<GearBase>();
        GearBase existingGear = gridManager.GetGearAt(gridPos);

        //Empty cell — just place gear
        if (existingGear == null)
        {
            Vector2Int previousPos = droppedGear.GridPosition;
            gridManager.RemoveGear(previousPos);
            gridManager.PlaceGear(droppedGear, gridPos);

            droppedGear.GridPosition = gridPos;
            gear.transform.position = gridManager.GridToWorld(gridPos);

            GearEventManager.Instance?.GearGridChanged();
            return true;
        }

        //Merge logic — same type, not motor, not character
        if (existingGear != droppedGear &&
            existingGear.gearType == droppedGear.gearType &&
            existingGear.gearType != GearType.Motor &&
            existingGear.gearType != GearType.Character)
        {
            int maxSubtype = GetMaxSubtype(existingGear.gearType);
            int nextSubtype = existingGear.Subtype + 1;

            if (nextSubtype <= maxSubtype)
            {
                // Remove old gears from grid
                gridManager.RemoveGear(droppedGear.GridPosition);
                gridManager.RemoveGear(existingGear.GridPosition);

                // Return to pool
                GearFactory.Instance.ReturnGearToPool(droppedGear);
                GearFactory.Instance.ReturnGearToPool(existingGear);

                // Create upgraded gear
                var newGearObj = GearFactory.Instance.GetPooledGear(existingGear.gearType, nextSubtype);
                var newGear = newGearObj.GetComponent<GearBase>();
                newGear.gearType = existingGear.gearType;
                newGear.Subtype = nextSubtype;

                // Place and set correct position
                gridManager.PlaceGear(newGear, gridPos);
                newGear.GridPosition = gridPos;
                newGear.transform.position = gridManager.GridToWorld(gridPos);

                GearEventManager.Instance?.GearGridChanged();
                return true;
            }
            else
            {
                gear.transform.position = originalWorldPos;
                return false;
            }
        }

        //Swap if not the same gear
        if (existingGear != droppedGear)
        {
            Vector2Int previousPos = droppedGear.GridPosition;
            gridManager.RemoveGear(previousPos);
            gridManager.RemoveGear(gridPos);

            gridManager.PlaceGear(droppedGear, gridPos);
            gridManager.PlaceGear(existingGear, previousPos);

            droppedGear.GridPosition = gridPos;
            existingGear.GridPosition = previousPos;

            gear.transform.position = gridManager.GridToWorld(gridPos);
            existingGear.transform.position = gridManager.GridToWorld(previousPos);

            GearEventManager.Instance?.GearGridChanged();
            return true;
        }

        // Dropped on itself → do nothing
        gear.transform.position = gridManager.GridToWorld(gridPos);
        return false;
    }

    private int GetMaxSubtype(GearType type)
    {
        if (type == GearType.Number)
            return GearFactory.Instance.numberGearPrefabs.Length - 1;
        else if (type == GearType.Multiplier)
            return GearFactory.Instance.multiplierGearPrefabs.Length - 1;
        return 0;
    }
}