using UnityEngine;

namespace GearSystem
{
    public class Motor : GearBase
    {
        private void Awake()
        {
            gearType = GearType.Motor;
        }

        private void Start()
        {
            StartRotation();
        }
    }
}