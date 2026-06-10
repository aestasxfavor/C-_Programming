using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipVisualController
{
    private readonly bool useOverlay;
    private readonly BoardRole boardRole;
    private readonly int boardSize;
    private readonly BoardCell[,] cells;

    private readonly RectTransform visualRoot;
    private readonly Image visualTemplate;

    private readonly Sprite shipSize2Sprite;
    private readonly Sprite shipSize3Sprite;
    private readonly Sprite shipSize4Sprite;
    private readonly Sprite shipSize5Sprite;

    private readonly Vector2 visualPadding;

    private readonly Dictionary<int, Image> visualByShipId = new Dictionary<int, Image>();

    public bool CanShowShipVisual
    {
        get
        {
            if (!useOverlay)
            {
                return false;
            }

            if (visualRoot == null)
            {
                return false;
            }

            if (visualTemplate == null)
            {
                return false;
            }

            return true;
        }
    }

    public ShipVisualController(
        bool _useOverlay,
        BoardRole _boardRole,
        int _boardSize,
        BoardCell[,] _cells,
        RectTransform _visualRoot,
        Image _visualTemplate,
        Sprite _shipSize2Sprite,
        Sprite _shipSize3Sprite,
        Sprite _shipSize4Sprite,
        Sprite _shipSize5Sprite,
        Vector2 _visualPadding
    )
    {
        useOverlay = _useOverlay;
        boardRole = _boardRole;
        boardSize = _boardSize;
        cells = _cells;
        visualRoot = _visualRoot;
        visualTemplate = _visualTemplate;
        shipSize2Sprite = _shipSize2Sprite;
        shipSize3Sprite = _shipSize3Sprite;
        shipSize4Sprite = _shipSize4Sprite;
        shipSize5Sprite = _shipSize5Sprite;
        visualPadding = _visualPadding;
    }

    public void InitVisualRoot()
    {
        if (visualTemplate != null)
        {
            visualTemplate.gameObject.SetActive(false);
            visualTemplate.raycastTarget = false;
        }

        FitRootToBoard();
        IgnoreLayoutOnRoot();
        BringRootToFront();
    }

    public void ShowShip(ShipData ship)
    {
        if (!CanShowShipVisual)
        {
            return;
        }

        if (ship == null)
        {
            return;
        }

        if (ship.positions == null || ship.positions.Count == 0)
        {
            return;
        }

        Sprite shipSprite = GetSpriteForShipSize(ship.size);

        if (shipSprite == null)
        {
            Debug.LogWarning($"[ShipVisual] Size={ship.size} 배 이미지 스프라이트가 없음");
            return;
        }

        BringRootToFront();
        RemoveShip(ship.shipId);

        Image visual = Object.Instantiate(visualTemplate, visualRoot);

        visual.gameObject.name = $"ShipVisual_{ship.shipId}_Size{ship.size}";
        visual.sprite = shipSprite;
        visual.raycastTarget = false;
        visual.gameObject.SetActive(true);

        RectTransform visualRect = visual.rectTransform;

        visualRect.anchorMin = new Vector2(0.5f, 0.5f);
        visualRect.anchorMax = new Vector2(0.5f, 0.5f);
        visualRect.pivot = new Vector2(0.5f, 0.5f);
        visualRect.localScale = Vector3.one;

        Vector2Int firstPosition = ship.positions[0];
        Vector2Int lastPosition = ship.positions[ship.positions.Count - 1];

        RectTransform firstCellRect = GetCellRectTransform(firstPosition);
        RectTransform lastCellRect = GetCellRectTransform(lastPosition);

        if (firstCellRect == null || lastCellRect == null)
        {
            Object.Destroy(visual.gameObject);
            return;
        }

        Vector3 centerWorldPosition = (firstCellRect.position + lastCellRect.position) * 0.5f;
        Vector2 centerRootPosition = WorldToRootPoint(centerWorldPosition);
        Vector2 cellSize = GetCellSizeInRoot(firstCellRect);

        bool isHorizontal = IsHorizontal(ship.positions);

        float visualLength;
        float visualThickness;

        if (isHorizontal)
        {
            visualLength = cellSize.x * ship.size + visualPadding.x;
            visualThickness = cellSize.y + visualPadding.y;
            visualRect.localRotation = Quaternion.identity;
        }
        else
        {
            visualLength = cellSize.y * ship.size + visualPadding.y;
            visualThickness = cellSize.x + visualPadding.x;
            visualRect.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        visualRect.anchoredPosition = centerRootPosition;
        visualRect.sizeDelta = new Vector2(visualLength, visualThickness);

        visualRect.SetAsLastSibling();
        BringRootToFront();

        visualByShipId[ship.shipId] = visual;

        Debug.Log($"[ShipVisual] 생성 완료: ShipID={ship.shipId}, Size={ship.size}, Direction={(isHorizontal ? "Horizontal" : "Vertical")}");
    }

    public void RemoveShip(int shipID)
    {
        if (!visualByShipId.TryGetValue(shipID, out Image visual))
        {
            return;
        }

        if (visual != null)
        {
            Object.Destroy(visual.gameObject);
        }

        visualByShipId.Remove(shipID);

        Debug.Log($"[ShipVisual] 제거 완료: ShipID={shipID}");
    }

    public void ClearAllShips()
    {
        foreach (KeyValuePair<int, Image> pair in visualByShipId)
        {
            if (pair.Value != null)
            {
                Object.Destroy(pair.Value.gameObject);
            }
        }

        visualByShipId.Clear();
    }

    private void FitRootToBoard()
    {
        if (visualRoot == null)
        {
            return;
        }

        visualRoot.anchorMin = Vector2.zero;
        visualRoot.anchorMax = Vector2.one;
        visualRoot.offsetMin = Vector2.zero;
        visualRoot.offsetMax = Vector2.zero;
        visualRoot.pivot = new Vector2(0.5f, 0.5f);
        visualRoot.localScale = Vector3.one;
        visualRoot.localRotation = Quaternion.identity;
    }

    private void IgnoreLayoutOnRoot()
    {
        if (visualRoot == null)
        {
            return;
        }

        LayoutElement layoutElement = visualRoot.GetComponent<LayoutElement>();

        if (layoutElement == null)
        {
            layoutElement = visualRoot.gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.ignoreLayout = true;
    }

    private void BringRootToFront()
    {
        if (visualRoot == null)
        {
            return;
        }

        visualRoot.SetAsLastSibling();
    }

    private Sprite GetSpriteForShipSize(int shipSize)
    {
        switch (shipSize)
        {
            case 2:
                return shipSize2Sprite;

            case 3:
                return shipSize3Sprite;

            case 4:
                return shipSize4Sprite;

            case 5:
                return shipSize5Sprite;

            default:
                return null;
        }
    }

    private RectTransform GetCellRectTransform(Vector2Int position)
    {
        if (!IsInsideBoard(position.x, position.y))
        {
            return null;
        }

        if (cells == null || cells[position.x, position.y] == null)
        {
            return null;
        }

        return cells[position.x, position.y].GetComponent<RectTransform>();
    }

    private bool IsHorizontal(List<Vector2Int> positions)
    {
        if (positions == null || positions.Count <= 1)
        {
            return true;
        }

        return positions[0].y == positions[positions.Count - 1].y;
    }

    private Vector2 WorldToRootPoint(Vector3 worldPosition)
    {
        Canvas canvas = visualRoot.GetComponentInParent<Canvas>();
        Camera camera = null;

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            camera = canvas.worldCamera;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            visualRoot,
            screenPoint,
            camera,
            out Vector2 rootPoint
        );

        return rootPoint;
    }

    private Vector2 GetCellSizeInRoot(RectTransform cellRect)
    {
        if (cellRect == null)
        {
            return Vector2.zero;
        }

        Vector3[] worldCorners = new Vector3[4];
        cellRect.GetWorldCorners(worldCorners);

        Vector2 bottomLeft = WorldToRootPoint(worldCorners[0]);
        Vector2 topRight = WorldToRootPoint(worldCorners[2]);

        float width = Mathf.Abs(topRight.x - bottomLeft.x);
        float height = Mathf.Abs(topRight.y - bottomLeft.y);

        return new Vector2(width, height);
    }

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
    }
}