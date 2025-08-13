using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace GearSystem
{
    public class GearStoreUI : MonoBehaviour
    {
        [SerializeField] private GearDragUI[] storeSlots;
        [SerializeField] private float fadeDuration = 0.15f;
        [SerializeField] private float popScale = 1.2f;
        [SerializeField] private float popDuration = 0.12f;

        private void Start()
        {
            // quick safety checks
            if (storeSlots == null || storeSlots.Length == 0)
            {
                Debug.LogError("[GearStoreUI] storeSlots not assigned.");
                return;
            }
            RefreshStore();
        }

        // Inspector-callable
        [ContextMenu("RefreshStore")]
        public void RefreshStore()
        {
            InternalRefreshLogic();
        }

        public void RefreshStoreAnimated()
        {
            // ensure all slots visible and kill existing tweens
            foreach (var s in storeSlots)
            {
                if (s == null) continue;
                s.gameObject.SetActive(true);
                if (s.IconImage != null)
                {
                    s.IconImage.DOKill();
                    s.IconImage.DOFade(0f, fadeDuration);
                    s.IconImage.rectTransform.DOKill();
                    s.IconImage.rectTransform.DOScale(1f, 0f); // reset
                }
            }

            DOVirtual.DelayedCall(fadeDuration, () =>
            {
                InternalRefreshLogic();

                // fade/pop in
                foreach (var s in storeSlots)
                {
                    if (s == null || s.IconImage == null) continue;
                    s.IconImage.DOFade(1f, fadeDuration);
                    s.IconImage.rectTransform.localScale = Vector3.one * 0.92f;
                    s.IconImage.rectTransform.DOScale(popScale, popDuration).SetEase(Ease.OutBack)
                        .OnComplete(() => s.IconImage.rectTransform.DOScale(1.5f, popDuration / 1.5f).SetEase(Ease.OutQuad));
                }
            });
        }

        private void InternalRefreshLogic()
        {
            if (GearFactory.Instance == null)
            {
                Debug.LogError("[GearStoreUI] GearFactory.Instance is null.");
                return;
            }

            int charSlotIndex = Random.Range(0, storeSlots.Length);

            for (int i = 0; i < storeSlots.Length; i++)
            {
                var slot = storeSlots[i];
                if (slot == null)
                {
                    Debug.LogWarning($"[GearStoreUI] storeSlots[{i}] is null.");
                    continue;
                }

                // ensure slot is active (we stopped disabling them)
                slot.gameObject.SetActive(true);

                GearType type;
                int subtype = 0;
                Sprite gearSprite = null;
                string subtypeName = "";

                if (i == charSlotIndex)
                {
                    type = GearType.Character;
                    subtype = Random.Range(0, GearFactory.Instance.characterGearPrefabs.Length);
                    gearSprite = GetSpriteFromPrefab(GearFactory.Instance.characterGearPrefabs[subtype]);
                    subtypeName = GearFactory.Instance.characterGearValues[subtype];
                }
                else
                {
                    // only Number or Multiplier allowed (no Motor)
                    if (Random.value < 0.5f)
                    {
                        type = GearType.Number;
                        subtype = Random.Range(0, GearFactory.Instance.numberGearPrefabs.Length);
                        gearSprite = GetSpriteFromPrefab(GearFactory.Instance.numberGearPrefabs[subtype]);
                        subtypeName = GearFactory.Instance.numberGearValues[subtype].ToString();
                    }
                    else
                    {
                        type = GearType.Multiplier;
                        subtype = Random.Range(0, GearFactory.Instance.multiplierGearPrefabs.Length);
                        gearSprite = GetSpriteFromPrefab(GearFactory.Instance.multiplierGearPrefabs[subtype]);
                        subtypeName = GearFactory.Instance.multiplierGearValues[subtype].ToString();
                    }
                }

                // ensure icon visible/opaque
                if (slot.IconImage != null)
                {
                    slot.IconImage.color = new Color(1f, 1f, 1f, 1f);
                }

                // assign to slot
                slot.SetupWithGear(type, subtype, gearSprite,subtypeName);

                Debug.Log($"[GearStoreUI] Slot {i} assigned => type:{type} subtype:{subtype} sprite:{(gearSprite ? gearSprite.name : "null")}");
            }
        }

        private Sprite GetSpriteFromPrefab(GameObject prefab)
        {
            if (prefab == null) return null;
            var sr = prefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) return sr.sprite;
            var uiImg = prefab.GetComponentInChildren<UnityEngine.UI.Image>();
            if (uiImg != null) return uiImg.sprite;
            return null;
        }
    }
}