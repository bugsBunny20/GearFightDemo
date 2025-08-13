using UnityEngine;
namespace GearSystem
{
    public enum MultiplierSubType { X1_25 = 0, X1_5 = 1, X2 = 2 }

    public class MultiplierGear : GearBase, ISubtypeProvider
    {
        [Header("Multiplier Settings")] 
        [SerializeField] private int subtypeIndex; // 0=1.25x, 1=1.5x, 2=2x

        public int GetSubtype() => subtypeIndex;
        private void Awake()
        {
            gearType = GearType.Multiplier;
        }

        public float GetMultiplier()
        {
            switch (subtypeIndex)
            {
                case (int)MultiplierSubType.X1_25: return 1.25f;
                case (int)MultiplierSubType.X1_5: return 1.5f;
                case (int)MultiplierSubType.X2: return 2f;
                default: return 1f;
            }
        }
    }
}