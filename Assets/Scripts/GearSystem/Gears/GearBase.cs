using UnityEngine;
using DG.Tweening;

namespace GearSystem
{
    public enum GearType
    {
        Motor,
        Character,
        Number,
        Multiplier
    }

    [RequireComponent(typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public class GearBase : MonoBehaviour
    {
        public GearType gearType;
        public int Subtype { get; set; }

        public Vector2Int GridPosition { get; set; }

        private SpriteRenderer spriteRenderer;

        [Header("Visual State")]
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;

        [Header("Sorting Order")]
        [SerializeField] private int normalSortingOrder = 2;
        [SerializeField] private int draggingSortingOrder = 15;

        private bool isActive;
        public bool IsActive => isActive;

        private Tween rotationTween;
        private bool isRotating = false;

        protected virtual void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetActive(bool value)
        {
            isActive = value;
            if (spriteRenderer != null && activeSprite != null && inactiveSprite != null)
            {
                spriteRenderer.sprite = isActive ? activeSprite : inactiveSprite;
            }
        }

        public void SetSortingOrder(bool isDragging)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = isDragging ? draggingSortingOrder : normalSortingOrder;
            }
        }

        public void SetSprite(Sprite newSprite)
        {
            if (spriteRenderer != null)
                spriteRenderer.sprite = newSprite;
        }

        public virtual void RotateOnce(bool clockwise)
        {
            if (isRotating) return;

            isRotating = true;
            float angleOffset = clockwise ? 25f : -25f;

            transform
                .DORotate(new Vector3(0, 0, angleOffset), 0.2f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => isRotating = false);
        }

        public void StartRotation()
        {
            if (rotationTween != null && rotationTween.IsActive())
                rotationTween.Kill();

            rotationTween = transform.DORotate(new Vector3(0, 0, -360), 2.5f, RotateMode.FastBeyond360)
                                      .SetEase(Ease.Linear)
                                      .SetLoops(-1);
        }

        public void StopRotation()
        {
            if (rotationTween != null && rotationTween.IsActive())
                rotationTween.Kill();
        }
    }
}