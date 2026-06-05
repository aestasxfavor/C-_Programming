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

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDroppedSuccessfully = false;
        startAnchoredPosition = rectTransform.anchoredPosition;

        boardView.SelectShip(shipID);

        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
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
        Debug.Log("[ShipDragItem] 靛而 己傍 贸府");
    }
}