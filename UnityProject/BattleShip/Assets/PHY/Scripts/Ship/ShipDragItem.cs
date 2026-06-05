using UnityEngine;
using UnityEngine.EventSystems;

public class ShipDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private BoardView boardView;
    [SerializeField] private int shipID;

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;

    private Vector2 startAnchoredPosition;
    private bool isDroppedSuccessfully;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private bool IsPlacementLocked()
    {
        return GameManager.Instance != null && GameManager.Instance.IsPlacementLocked;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsPlacementLocked())
        {
            Debug.Log("[Placement] Ready 이후 드래그 시작 불가");
            return;
        }

        isDroppedSuccessfully = false;
        startAnchoredPosition = rectTransform.anchoredPosition;

        boardView.SelectShip(shipID);

        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsPlacementLocked())
        {
            return;
        }

        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsPlacementLocked())
        {
            canvasGroup.blocksRaycasts = true;
            return;
        }

        canvasGroup.blocksRaycasts = true;

        if (isDroppedSuccessfully)
        {
            gameObject.SetActive(false);
            return;
        }

        rectTransform.anchoredPosition = startAnchoredPosition;
    }

    public void MarkDroppedSuccessfully()
    {
        isDroppedSuccessfully = true;
        Debug.Log("[ShipDragItem] 드랍 성공 처리");
    }
}