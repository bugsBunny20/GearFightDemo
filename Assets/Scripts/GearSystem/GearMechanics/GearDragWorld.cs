using UnityEngine;
namespace GearSystem
{
    public class GearDragWorld : MonoBehaviour
    {
        private Vector3 offset;
        private Vector3 originalPosition;
        private bool isDragging = false;
        private GearBase gearBase;

        private void Start()
        {
            gearBase = GetComponent<GearBase>();
        }

        private void OnMouseDown()
        {
            originalPosition = transform.position;
            offset = transform.position - GetMouseWorldPosition();

            isDragging = true;
            gearBase.SetSortingOrder(isDragging);
        }

        private void OnMouseDrag()
        {
            if (isDragging)
            {
                transform.position = GetMouseWorldPosition() + offset;
            }
        }

        private void OnMouseUp()
        {
            isDragging = false;
            gearBase.SetSortingOrder(isDragging);
            GearPlacementHandler.Instance.TryPlaceDraggedGear(this, originalPosition);
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f; // Adjust based on camera's z-position if needed
            return Camera.main.ScreenToWorldPoint(mousePos);
        }
    }
}