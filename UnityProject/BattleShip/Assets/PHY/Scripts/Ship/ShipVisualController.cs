using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipVisualController
{
    private readonly bool useShipVisualOverlay;
    private readonly BoardRole boardRole;
    private readonly int boardSize;
    private readonly BoardCell[,] cells;
    private readonly RectTransform shipVisualRoot;
    private readonly Image shipVisualTemplate;
    private readonly Sprite shipSize2VisualSprite;
    private readonly Sprite shipSize3VisualSprite;
    private readonly Sprite shipSize4VisualSprite;
    private readonly Sprite shipSize5VisualSprite;
    private readonly Vector2 shipVisualPadding;

    private readonly Dictionary<int, Image> shipVisualsByID = new Dictionary<int, Image>();

    public bool IsEnabled
    {
        get
        {
            if (!useShipVisualOverlay)
            {
                return false;
            }

            if (boardRole != BoardRole.MyBoard)
            {
                return false;
            }

            if (shipVisualRoot == null)
            {
                return false;
            }

            if (shipVisualTemplate == null)
            {
                return false;
            }

            return true;
        }
    }

    public ShipVisualController(
        bool useShipVisualOverlay,
        BoardRole boardRole,
        int boardSize,
        BoardCell[,] cells,
        RectTransform shipVisualRoot,
        Image shipVisualTemplate,
        Sprite shipSize2VisualSprite,
        Sprite shipSize3VisualSprite,
        Sprite shipSize4VisualSprite,
        Sprite shipSize5VisualSprite,
        Vector2 shipVisualPadding
    )
    {
        this.useShipVisualOverlay = useShipVisualOverlay;
        this.boardRole = boardRole;
        this.boardSize = boardSize;
        this.cells = cells;
        this.shipVisualRoot = shipVisualRoot;
        this.shipVisualTemplate = shipVisualTemplate;
        this.shipSize2VisualSprite = shipSize2VisualSprite;
        this.shipSize3VisualSprite = shipSize3VisualSprite;
        this.shipSize4VisualSprite = shipSize4VisualSprite;
        this.shipSize5VisualSprite = shipSize5VisualSprite;
        this.shipVisualPadding = shipVisualPadding;
    }

    public void Init()
    {
        if (shipVisualTemplate != null)
        {
            shipVisualTemplate.gameObject.SetActive(false);
            shipVisualTemplate.raycastTarget = false;
        }

        SetupRootRect();
        EnsureRootIgnoresLayout();
        MoveRootToFront();
    }

    public void Create(ShipData ship)
    {
        if (!IsEnabled)
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

        Sprite visualSprite = GetShipVisualSprite(ship.size);

        if (visualSprite == null)
        {
            Debug.LogWarning($"[ShipVisual] Size={ship.size} 배 이미지 스프라이트가 없음");
            return;
        }

        MoveRootToFront();
        Remove(ship.shipID);

        Image visual = Object.Instantiate(shipVisualTemplate, shipVisualRoot);

        visual.gameObject.name = $"ShipVisual_{ship.shipID}_Size{ship.size}";
        visual.sprite = visualSprite;
        visual.raycastTarget = false;
        visual.gameObject.SetActive(true);

        RectTransform visualRect = visual.rectTransform;

        visualRect.anchorMin = new Vector2(0.5f, 0.5f);
        visualRect.anchorMax = new Vector2(0.5f, 0.5f);
        visualRect.pivot = new Vector2(0.5f, 0.5f);
        visualRect.localScale = Vector3.one;

        Vector2Int firstPosition = ship.positions[0];
        Vector2Int lastPosition = ship.positions[ship.positions.Count - 1];

        RectTransform firstCellRect = GetCellRect(firstPosition);
        RectTransform lastCellRect = GetCellRect(lastPosition);

        if (firstCellRect == null || lastCellRect == null)
        {
            Object.Destroy(visual.gameObject);
            return;
        }

        Vector3 centerWorldPosition = (firstCellRect.position + lastCellRect.position) * 0.5f;
        Vector2 centerLocalPosition = WorldToRootLocalPoint(centerWorldPosition);
        Vector2 cellSize = GetCellSizeInRoot(firstCellRect);

        bool isHorizontal = IsHorizontalShip(ship.positions);

        float length;
        float thickness;

        if (isHorizontal)
        {
            length = cellSize.x * ship.size + shipVisualPadding.x;
            thickness = cellSize.y + shipVisualPadding.y;
            visualRect.localRotation = Quaternion.identity;
        }
        else
        {
            length = cellSize.y * ship.size + shipVisualPadding.y;
            thickness = cellSize.x + shipVisualPadding.x;
            visualRect.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        visualRect.anchoredPosition = centerLocalPosition;
        visualRect.sizeDelta = new Vector2(length, thickness);

        visualRect.SetAsLastSibling();
        MoveRootToFront();

        shipVisualsByID[ship.shipID] = visual;

        Debug.Log($"[ShipVisual] 생성 완료: ShipID={ship.shipID}, Size={ship.size}, Direction={(isHorizontal ? "Horizontal" : "Vertical")}");
    }

    public void Remove(int shipID)
    {
        if (!shipVisualsByID.TryGetValue(shipID, out Image visual))
        {
            return;
        }

        if (visual != null)
        {
            Object.Destroy(visual.gameObject);
        }

        shipVisualsByID.Remove(shipID);

        Debug.Log($"[ShipVisual] 제거 완료: ShipID={shipID}");
    }

    public void ClearAll()
    {
        foreach (KeyValuePair<int, Image> pair in shipVisualsByID)
        {
            if (pair.Value != null)
            {
                Object.Destroy(pair.Value.gameObject);
            }
        }

        shipVisualsByID.Clear();
    }

    private void SetupRootRect()
    {
        if (shipVisualRoot == null)
        {
            return;
        }

        shipVisualRoot.anchorMin = Vector2.zero;
        shipVisualRoot.anchorMax = Vector2.one;
        shipVisualRoot.offsetMin = Vector2.zero;
        shipVisualRoot.offsetMax = Vector2.zero;
        shipVisualRoot.pivot = new Vector2(0.5f, 0.5f);
        shipVisualRoot.localScale = Vector3.one;
        shipVisualRoot.localRotation = Quaternion.identity;
    }

    private void EnsureRootIgnoresLayout()
    {
        if (shipVisualRoot == null)
        {
            return;
        }

        LayoutElement layoutElement = shipVisualRoot.GetComponent<LayoutElement>();

        if (layoutElement == null)
        {
            layoutElement = shipVisualRoot.gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.ignoreLayout = true;
    }

    private void MoveRootToFront()
    {
        if (shipVisualRoot == null)
        {
            return;
        }

        shipVisualRoot.SetAsLastSibling();
    }

    private Sprite GetShipVisualSprite(int shipSize)
    {
        switch (shipSize)
        {
            case 2:
                return shipSize2VisualSprite;

            case 3:
                return shipSize3VisualSprite;

            case 4:
                return shipSize4VisualSprite;

            case 5:
                return shipSize5VisualSprite;

            default:
                return null;
        }
    }

    private RectTransform GetCellRect(Vector2Int position)
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

    private bool IsHorizontalShip(List<Vector2Int> positions)
    {
        if (positions == null || positions.Count <= 1)
        {
            return true;
        }

        return positions[0].y == positions[positions.Count - 1].y;
    }

    private Vector2 WorldToRootLocalPoint(Vector3 worldPosition)
    {
        Canvas canvas = shipVisualRoot.GetComponentInParent<Canvas>();
        Camera camera = null;

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            camera = canvas.worldCamera;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            shipVisualRoot,
            screenPoint,
            camera,
            out Vector2 localPoint
        );

        return localPoint;
    }

    private Vector2 GetCellSizeInRoot(RectTransform cellRect)
    {
        if (cellRect == null)
        {
            return Vector2.zero;
        }

        Vector3[] worldCorners = new Vector3[4];
        cellRect.GetWorldCorners(worldCorners);

        Vector2 bottomLeft = WorldToRootLocalPoint(worldCorners[0]);
        Vector2 topRight = WorldToRootLocalPoint(worldCorners[2]);

        float width = Mathf.Abs(topRight.x - bottomLeft.x);
        float height = Mathf.Abs(topRight.y - bottomLeft.y);

        return new Vector2(width, height);
    }

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
    }
}