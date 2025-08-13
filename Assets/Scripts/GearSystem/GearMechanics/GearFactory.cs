using System.Collections.Generic;
using UnityEngine;

namespace GearSystem
{
    public class GearFactory : MonoBehaviour
    {
        public static GearFactory Instance;

        [Header("Number Gear Prefabs")]
        public GameObject[] numberGearPrefabs; // index = subtype
        public int[] numberGearValues = { 1, 2, 4, 8 };

        [Header("Multiplier Gear Prefabs")]
        public GameObject[] multiplierGearPrefabs; // index = subtype
        public float[] multiplierGearValues = { 1.25f, 1.5f, 2f };

        [Header("Character Gear Prefabs")]
        public GameObject[] characterGearPrefabs;
        public string[] characterGearValues = { "R", "S" };

        [Header("Other Gear Prefabs")]
        public GameObject motorGearPrefab;

        private Dictionary<GearType, Dictionary<int, Queue<GameObject>>> pool =
            new Dictionary<GearType, Dictionary<int, Queue<GameObject>>>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            foreach (GearType type in System.Enum.GetValues(typeof(GearType)))
                pool[type] = new Dictionary<int, Queue<GameObject>>();
        }

        public GameObject GetPooledGear(GearType type, int subtype)
        {
            if (!pool[type].ContainsKey(subtype))
                pool[type][subtype] = new Queue<GameObject>();

            if (pool[type][subtype].Count > 0)
            {
                var gear = pool[type][subtype].Dequeue();
                gear.SetActive(true);
                return gear;
            }
            else
            {
                return CreateGear(type, subtype);
            }
        }

        public void ReturnGearToPool(GearBase gear)
        {
            gear.gameObject.SetActive(false);
            if (!pool[gear.gearType].ContainsKey(gear.Subtype))
                pool[gear.gearType][gear.Subtype] = new Queue<GameObject>();

            pool[gear.gearType][gear.Subtype].Enqueue(gear.gameObject);
        }

        private GameObject CreateGear(GearType type, int subtype)
        {
            GameObject prefab = null;

            switch (type)
            {
                case GearType.Number:
                    prefab = numberGearPrefabs[subtype];
                    break;
                case GearType.Multiplier:
                    prefab = multiplierGearPrefabs[subtype];
                    break;
                case GearType.Character:
                    prefab = characterGearPrefabs[subtype];
                    break;
                case GearType.Motor:
                    prefab = motorGearPrefab;
                    break;
            }

            var obj = Instantiate(prefab);
            var gear = obj.GetComponent<GearBase>();
            gear.gearType = type;
            gear.Subtype = subtype;
            return obj;
        }

        public GameObject GetPrefab(GearType type, int subtype)
        {
            switch (type)
            {
                case GearType.Number:
                    return numberGearPrefabs[subtype];
                case GearType.Multiplier:
                    return multiplierGearPrefabs[subtype];
                case GearType.Character:
                    return characterGearPrefabs[subtype];
                case GearType.Motor:
                    return motorGearPrefab;
            }
            return null;
        }
    }
}