using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using GearSystem;

public class GearDragUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text subtypeText;

    public Image IconImage => iconImage; //used by GearStoreUI
    public Text SubtypeText => subtypeText; //used by GearStoreU

    private CanvasGroup canvasGroup;

    private GearType gearType;
    private int gearSubtype;

    private GameObject spawnedGear;
    private bool isDragging;
    private Vector3 dragStartWorldPos;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // Called by GearStoreUI
    public void SetupWithGear(GearType type, int subtype, Sprite gearSprite, string subtypeName)
    {
        gearType = type;
        gearSubtype = subtype;

        if (iconImage != null)
        {
            iconImage.sprite = gearSprite;
            iconImage.color = new Color(1f, 1f, 1f, 1f); // ensure visible
        }
        else
        {
            Debug.LogWarning($"[GearDragUI] Slot {name} missing iconImage reference.");
        }

        if(subtypeText.text != null)
        {
            subtypeText.text = subtypeName;
            subtypeText.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gearType == default && iconImage != null && iconImage.sprite == null)
        {
            // nothing assigned, block drag
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        spawnedGear = GearFactory.Instance.GetPooledGear(gearType, gearSubtype);
        spawnedGear.transform.position = mouseWorld;

        dragStartWorldPos = mouseWorld;
        isDragging = true;
        spawnedGear.GetComponent<GearBase>().SetSortingOrder(isDragging);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (spawnedGear == null) return;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        spawnedGear.transform.position = mouseWorld;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (spawnedGear == null)
        {
            // restore UI visibility (in case OnBeginDrag failed)
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            return;
        }

        isDragging = false;

        bool placed = GearPlacementHandler.Instance.TryPlaceDraggedGear(
            spawnedGear.GetComponent<GearDragWorld>(),
            spawnedGear.transform.position
        );

        if (!placed)
        {
            // failed placement: tween world gear back to drag start pos, then return to pool and restore slot UI
            spawnedGear.transform.DOMove(dragStartWorldPos, 0.22f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // return to pool
                GearFactory.Instance.ReturnGearToPool(spawnedGear.GetComponent<GearBase>());
                spawnedGear = null;

                // restore UI
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            });
        }
        else
        {
            // successful purchase/placement(visuals are disabled not game object)
            spawnedGear.GetComponent<GearBase>().SetSortingOrder(isDragging);

            // hide icon until next refresh
            if (iconImage != null)
            {
                iconImage.color = new Color(1f, 1f, 1f, 0f);
            }
            if (subtypeText != null)
            {
                subtypeText.color = new Color(1f, 1f, 1f, 0f);
            }

            // clear stored data so user can't drag same slot again until refresh
            gearType = default;
            gearSubtype = 0;
            spawnedGear = null;

            // make slot still interactive for refresh
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }
}