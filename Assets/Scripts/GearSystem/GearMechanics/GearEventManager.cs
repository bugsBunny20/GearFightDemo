using System;
using UnityEngine;

namespace GearSystem
{
    public class GearEventManager : MonoBehaviour
    {
        public static GearEventManager Instance;
        public event Action OnGearGridChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void GearGridChanged()
        {
            OnGearGridChanged?.Invoke();
        }
    }
}