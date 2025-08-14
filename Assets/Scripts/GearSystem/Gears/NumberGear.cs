using UnityEngine;

public enum NumberSubType { One = 0, Two = 1, Four = 2, Eight = 3 }

namespace GearSystem
{
    public class NumberGear : GearBase
    {
        [Header("Number Settings")]
        [SerializeField] private int subtypeIndex; // 0=1, 1=2, 2=4, 3=8

        public float GetSubtype() => subtypeIndex;

        private void Awake()
        {
            gearType = GearType.Number;
        }

        public float GetValue()
        {
            switch (subtypeIndex)
            {
                case (int)NumberSubType.One: return 0.06f;
                case (int)NumberSubType.Two: return 0.11f;
                case (int)NumberSubType.Four: return 0.17f;
                case (int)NumberSubType.Eight: return 0.22f;
                default: return 0.25f;
            }
        }
    }
}