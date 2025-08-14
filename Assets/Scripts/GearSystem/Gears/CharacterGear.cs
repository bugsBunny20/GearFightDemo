using UnityEngine;
using System.Collections.Generic;
using GearSystem;
using TMPro;

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

    [SerializeField] private TextMeshPro fillerValueText;
    [SerializeField] private SpriteRenderer fillerBackground;

    public int GetSubtype() => subtypeIndex;

    protected override void OnEnable()
    {
        base.OnEnable();
        // Subscribe to path change
        if (fillerValueText != null)
        {
            fillerValueText.sortingOrder = normalSortingOrder + 2;
            fillerValueText.text = fillerValue.ToString() + "/s";
        }
        if (GridManager.Instance != null)
            GridManager.Instance.OnActivePathChanged += OnActivePathChanged;
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
            GridManager.Instance.OnActivePathChanged -= OnActivePathChanged;
    }


    public override void SetSortingOrder(bool isDragging)
    {
        base.SetSortingOrder(isDragging);
        if (fillerValueText != null)
        {
            fillerValueText.sortingOrder = isDragging ? draggingSortingOrder + 2 : normalSortingOrder + 2;
        }
        if (fillerBackground != null)
        {
            fillerBackground.sortingOrder = isDragging ? draggingSortingOrder + 1 : normalSortingOrder + 1;
        }
    }

    // Called by grid when active path changes
    private void OnActivePathChanged(HashSet<Vector2Int> activePath)
    {
        GridManager gridManager = GridManager.Instance;
        if (gridManager == null) return;

        int activeMotorCount = 0;
        float additiveBonus = 0f;
        float totalMultiplier = 1f;

        foreach (var pos in activePath)
        {
            GearBase gear = gridManager.GetGearAt(pos);
            if (gear == null) continue;

            // Count active motors
            if (gear.gearType == GearType.Motor)
                activeMotorCount++;

            // Add bonuses
            if (gear is NumberGear num)
                additiveBonus += num.GetValue();
            else if (gear is MultiplierGear mult)
                totalMultiplier *= mult.GetMultiplier();
        }

        if (activeMotorCount == 0)
        {
            calculatedFillPerRotation = 0f;
            return;
        }

        calculatedFillPerRotation = (baseFillPerRotation + activeMotorCount * additiveBonus) * totalMultiplier;
        fillerValueText.text = calculatedFillPerRotation.ToString() +"/s";

        Debug.Log($"[CharacterGear] Active Motors: {activeMotorCount}, Fill/Rotation: {calculatedFillPerRotation}");
    }

    // Called once per rotation
    public void AdvanceProgress()
    {
        if (!GameManager.Instance.GameStarted) return;

        if (!IsActive) return;

        if (calculatedFillPerRotation <= 0f) return;

        fillerValue += calculatedFillPerRotation;
        fillerValueText.text = fillerValue.ToString()+"/s";

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

    public float GetCurrentFill() => fillerValue;
    public float GetCalculatedPerRotation() => calculatedFillPerRotation;
}