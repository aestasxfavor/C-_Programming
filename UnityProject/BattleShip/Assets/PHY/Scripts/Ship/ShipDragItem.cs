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
    private bool hasStartPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        SaveStartPosition();
    }

    private void Start()
    {
        SaveStartPosition();
    }

    private void SaveStartPosition()
    {
        if (rectTransform == null)
        {
            return;
        }

        if (hasStartPosition)
        {
            return;
        }

        startAnchoredPosition = rectTransform.anchoredPosition;
        hasStartPosition = true;
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

        SaveStartPosition();

        isDroppedSuccessfully = false;

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

    public void ResetDragItem()
    {
        SaveStartPosition();

        isDroppedSuccessfully = false;

        gameObject.SetActive(true);

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startAnchoredPosition;
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
        }

        Debug.Log($"[ShipDragItem] 리셋 완료: ShipID={shipID}");
    }
}