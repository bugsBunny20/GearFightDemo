using GearSystem;
using System.Collections.Generic;
using UnityEngine;

namespace GearSystem
{
    public class MotorRotatorPoint : MonoBehaviour
    {
        private HashSet<GearBase> triggeredGears = new HashSet<GearBase>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out GearBase gear))
            {
                GearRotator.Instance.RotateConnectedGears(gear.GridPosition);
            }
        }

        private void LateUpdate()
        {
            triggeredGears.Clear();
        }
    }
}