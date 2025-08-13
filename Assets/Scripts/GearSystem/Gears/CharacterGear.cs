using UnityEngine;
using System.Collections.Generic;
using GearSystem;

public enum CharacterSubType { Round = 0, Square = 1 }

public class CharacterGear : GearBase, ISubtypeProvider
{
    [Header("Character Setup")]
    [SerializeField] private int subtypeIndex = 0;
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private Transform spawnPoint;  // optional position for spawn

    [Header("Filler / per-rotation")]
    [Tooltip("Base fill added per rotation (e.g. 0.21)")]
    [SerializeField] private float baseFillPerRotation = 0.21f;

    private float calculatedFillPerRotation = 0f; // computed from active path
    private float fillerValue = 0f;
    [SerializeField] private float fillerThreshold = 1f; // spawn when >= 1

    public int GetSubtype() => subtypeIndex;

    protected override void OnEnable()
    {
        base.OnEnable();
        // Subscribe to path change
        if (GridManager.Instance != null)
            GridManager.Instance.OnActivePathChanged += RecalculateFillPerRotation;
        baseFillPerRotation = GetBaseFillValue((CharacterSubType)subtypeIndex);
    }

    float GetBaseFillValue(CharacterSubType characterSubType) => characterSubType switch
    {
        CharacterSubType.Round => 0.2f,
        CharacterSubType.Square => 0.25f,
        _ => 0.21f
    };

    private void OnDisable()
    {
        if (GridManager.Instance != null)
            GridManager.Instance.OnActivePathChanged -= RecalculateFillPerRotation;
    }

    // Called by grid when active path changes
    private void RecalculateFillPerRotation(HashSet<Vector2Int> activePath)
    {
        float additiveBonus = 0f;
        float totalMultiplier = 1f;

        GridManager grid = GridManager.Instance;
        if (grid == null)
        {
            calculatedFillPerRotation = 0f;
            return;
        }

        foreach (var pos in activePath)
        {
            GearBase gear = grid.GetGearAt(pos);
            if (gear == null || gear == this) continue;

            if (gear is NumberGear num)
                additiveBonus += num.GetValue();
            else if (gear is MultiplierGear mult)
                totalMultiplier *= mult.GetMultiplier();
        }

        // final per-rotation fill
        calculatedFillPerRotation = (baseFillPerRotation + additiveBonus) * totalMultiplier;

        if (calculatedFillPerRotation < 0f) calculatedFillPerRotation = 0f;

        Debug.Log($"[CharacterGear] calcPerRot={calculatedFillPerRotation:F4}");
    }

    // Called once per rotation
    public void AdvanceProgress()
    {
        if (!GameManager.Instance.GameStarted) return;

        if (!IsActive) return;

        if (calculatedFillPerRotation <= 0f) return;

        fillerValue += calculatedFillPerRotation;

        if (fillerValue >= fillerThreshold)
        {
   
            SpawnCharacter();
            fillerValue = 0f;
        }
    }

    private void SpawnCharacter()
    {
        Debug.Log("[CharacterGear] Character spawned");
        if (characterPrefab != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(characterPrefab, pos, Quaternion.identity);
        }
    }

    // Optional debug helper
    public float GetCurrentFill() => fillerValue;
    public float GetCalculatedPerRotation() => calculatedFillPerRotation;
}