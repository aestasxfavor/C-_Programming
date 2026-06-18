using TMPro;
using UnityEngine;
using UnityEngine.UI;
using InventoryFramework;

public class VendorUIController : MonoBehaviour
{
    public static VendorUIController instance;

    [System.Serializable]
    private class OreSellRow
    {
        public Item item;
        public int price;

        [Header("Texts")]
        public TMP_Text nameText;
        public TMP_Text countText;
        public TMP_Text unitPriceText;
        public TMP_Text subTotalText;
    }

    [Header("Root")]
    [SerializeField] private GameObject vendorUIRoot;

    [Header("Panels")]
    [SerializeField] private GameObject oreSalesPanel;
    [SerializeField] private GameObject itemBuyPanel;

    [Header("Tab Buttons")]
    [SerializeField] private Button mineralSalesButton;
    [SerializeField] private Button itemBuyButton;

    [Header("Common Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button sellButton;

    [Header("Item Buy Buttons")]
    [SerializeField] private Button pickaxeBuyButton;
    [SerializeField] private Button pendantBuyButton;

    [Header("Ore Sell Rows")]
    [SerializeField] private OreSellRow[] oreSellRows;

    [Header("Total")]
    [SerializeField] private TMP_Text totalPriceText;

    [Header("UI Refresh")]
    [SerializeField] private HotbarUI hotbarUI;
    [SerializeField] private InventoryUI inventoryUI;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (mineralSalesButton != null)
        {
            mineralSalesButton.onClick.AddListener(ShowOreSalesPanel);
        }

        if (itemBuyButton != null)
        {
            itemBuyButton.onClick.AddListener(ShowItemBuyPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnClickSellButton);
        }
    }

    private void Start()
    {
        SetupItemBuyButtonsAsNotReady();
        Close();
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open()
    {
        IsOpen = true;

        if (vendorUIRoot != null)
        {
            vendorUIRoot.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowOreSalesPanel();
    }

    public void Close()
    {
        IsOpen = false;

        if (vendorUIRoot != null)
        {
            vendorUIRoot.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowOreSalesPanel()
    {
        if (oreSalesPanel != null)
        {
            oreSalesPanel.SetActive(true);
        }

        if (itemBuyPanel != null)
        {
            itemBuyPanel.SetActive(false);
        }

        RefreshOreSalesUI();
    }

    private void ShowItemBuyPanel()
    {
        if (oreSalesPanel != null)
        {
            oreSalesPanel.SetActive(false);
        }

        if (itemBuyPanel != null)
        {
            itemBuyPanel.SetActive(true);
        }
        else
        {
            Debug.Log("아이템 구매 패널이 연결되어 있지 않습니다.");
        }

        SetupItemBuyButtonsAsNotReady();
    }

    private void SetupItemBuyButtonsAsNotReady()
    {
        SetBuyButtonNotReady(pickaxeBuyButton);
        SetBuyButtonNotReady(pendantBuyButton);
    }

    private void SetBuyButtonNotReady(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = false;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(true);

        if (buttonText != null)
        {
            buttonText.text = "준비중";
        }
    }

    private void OnClickSellButton()
    {
        InventorySystem inventorySystem = InventorySystem.instance;

        if (inventorySystem == null)
        {
            Debug.LogError("InventorySystem이 없습니다.");
            return;
        }

        Hotbar hotbar = inventorySystem.Hotbar;
        Inventory inventory = inventorySystem.Inventory;

        if (hotbar == null || inventory == null)
        {
            Debug.LogError("Hotbar 또는 Inventory 참조가 없습니다.");
            return;
        }

        int totalPrice = CalculateTotalPrice(hotbar, inventory);

        if (totalPrice <= 0)
        {
            Debug.Log("판매할 광석이 없습니다.");
            RefreshOreSalesUI();
            return;
        }

        SellAllOres(hotbar, inventory);

        if (CoinManager.instance != null)
        {
            CoinManager.instance.AddCoin(totalPrice);
        }
        else
        {
            Debug.LogError("CoinManager가 없습니다.");
        }

        RefreshInventoryUIs();
        RefreshOreSalesUI();

        Debug.Log($"광석 전체 판매 완료: +{totalPrice} Y");
    }

    private int CalculateTotalPrice(Hotbar hotbar, Inventory inventory)
    {
        int totalPrice = 0;

        foreach (OreSellRow row in oreSellRows)
        {
            if (row == null || row.item == null)
            {
                continue;
            }

            int count = GetTotalItemCount(hotbar, inventory, row.item);
            totalPrice += count * row.price;
        }

        return totalPrice;
    }

    private void SellAllOres(Hotbar hotbar, Inventory inventory)
    {
        foreach (OreSellRow row in oreSellRows)
        {
            if (row == null || row.item == null)
            {
                continue;
            }

            int hotbarCount = hotbar.GetItemCount(row.item);
            int inventoryCount = inventory.GetItemCount(row.item);

            if (hotbarCount > 0)
            {
                hotbar.RemoveItem(row.item, hotbarCount);
            }

            if (inventoryCount > 0)
            {
                inventory.RemoveItem(row.item, inventoryCount);
            }
        }
    }

    private void RefreshOreSalesUI()
    {
        InventorySystem inventorySystem = InventorySystem.instance;

        if (inventorySystem == null)
        {
            SetTotalPriceText(0);
            return;
        }

        Hotbar hotbar = inventorySystem.Hotbar;
        Inventory inventory = inventorySystem.Inventory;

        if (hotbar == null || inventory == null)
        {
            SetTotalPriceText(0);
            return;
        }

        int totalPrice = 0;

        foreach (OreSellRow row in oreSellRows)
        {
            if (row == null || row.item == null)
            {
                continue;
            }

            int count = GetTotalItemCount(hotbar, inventory, row.item);
            int subTotal = count * row.price;

            totalPrice += subTotal;

            if (row.nameText != null)
            {
                row.nameText.text = row.item.itemName;
            }

            if (row.countText != null)
            {
                row.countText.text = $"보유 {count}개";
            }

            if (row.unitPriceText != null)
            {
                row.unitPriceText.text = $"개당 {row.price} Y";
            }

            if (row.subTotalText != null)
            {
                row.subTotalText.text = $"판매가 {subTotal} Y";
            }
        }

        SetTotalPriceText(totalPrice);
    }

    private int GetTotalItemCount(Hotbar hotbar, Inventory inventory, Item item)
    {
        int hotbarCount = hotbar.GetItemCount(item);
        int inventoryCount = inventory.GetItemCount(item);

        return hotbarCount + inventoryCount;
    }

    private void SetTotalPriceText(int totalPrice)
    {
        if (totalPriceText != null)
        {
            totalPriceText.text = $"총 판매가: {totalPrice} Y";
        }
    }

    private void RefreshInventoryUIs()
    {
        if (hotbarUI != null)
        {
            hotbarUI.RefreshUI();
        }

        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }
    }
}