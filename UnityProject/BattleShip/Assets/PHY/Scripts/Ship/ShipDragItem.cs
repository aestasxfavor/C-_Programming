using UnityEngine;
using UnityEngine.EventSystems;

// 배치 단계에서 함선 UI 드래그, 드랍 성공 처리, 원위치 복귀를 담당하는 스크립트
public class ShipDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private BoardView boardView;

    // TODO: shipId와 함선 크기 매핑은 추후 ShipDefinitionSO로 분리 가능
    [SerializeField] private int shipId;

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;

    private Vector2 startAnchoredPosition;
    private bool isDropSuccessful;
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

        isDropSuccessful = false;

        boardView.SelectShip(shipId);

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

    // 드랍 실패 시 시작 위치로 복귀, 성공 시 선택 UI 비활성화
    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsPlacementLocked())
        {
            canvasGroup.blocksRaycasts = true;
            return;
        }

        canvasGroup.blocksRaycasts = true;

        if (isDropSuccessful)
        {
            gameObject.SetActive(false);
            return;
        }

        rectTransform.anchoredPosition = startAnchoredPosition;
    }

    public void MarkDroppedSuccessfully()
    {
        isDropSuccessful = true;
        Debug.Log("[ShipDragItem] 드랍 성공 처리");
    }

    public void ResetDragItem()
    {
        SaveStartPosition();

        isDropSuccessful = false;

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

        Debug.Log($"[ShipDragItem] 리셋 완료: ShipID={shipId}");
    }
}