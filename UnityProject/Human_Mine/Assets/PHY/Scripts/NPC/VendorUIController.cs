using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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

    [Header("Input")]
    [SerializeField] private InputActionReference cancelAction;

    [Header("Panels")]
    [SerializeField] private GameObject oreSalesPanel;
    [SerializeField] private GameObject itemBuyPanel;
    [SerializeField] private GameObject questPanel;

    [Header("Tab Buttons")]
    [SerializeField] private Button mineralSalesButton;
    [SerializeField] private Button itemBuyButton;
    [SerializeField] private Button questButton;

    [Header("Common Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button sellButton;

    [Header("Quest")]
    [SerializeField] private Button questRewardButton;
    [SerializeField] private TMP_Text questProgressText;

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

    private bool enabledCancelActionHere;

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

        if (questButton != null)
        {
            questButton.onClick.AddListener(ShowQuestPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnClickSellButton);
        }

        if (questRewardButton != null)
        {
            questRewardButton.onClick.AddListener(OnClickQuestRewardButton);
        }
    }

    private void OnEnable()
    {
        if (cancelAction != null &&
            cancelAction.action != null &&
            !cancelAction.action.enabled)
        {
            cancelAction.action.Enable();
            enabledCancelActionHere = true;
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

        UnlockCursor();

        if (cancelAction == null || cancelAction.action == null)
        {
            return;
        }

        if (cancelAction.action.WasPressedThisFrame())
        {
            Close();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        if (mineralSalesButton != null)
        {
            mineralSalesButton.onClick.RemoveListener(ShowOreSalesPanel);
        }

        if (itemBuyButton != null)
        {
            itemBuyButton.onClick.RemoveListener(ShowItemBuyPanel);
        }

        if (questButton != null)
        {
            questButton.onClick.RemoveListener(ShowQuestPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }

        if (sellButton != null)
        {
            sellButton.onClick.RemoveListener(OnClickSellButton);
        }

        if (questRewardButton != null)
        {
            questRewardButton.onClick.RemoveListener(OnClickQuestRewardButton);
        }

        if (cancelAction != null &&
            cancelAction.action != null &&
            enabledCancelActionHere)
        {
            cancelAction.action.Disable();
            enabledCancelActionHere = false;
        }
    }

    public void Open()
    {
        IsOpen = true;

        if (vendorUIRoot != null)
        {
            vendorUIRoot.SetActive(true);
        }

        UnlockCursor();
        ShowOreSalesPanel();
    }

    public void Close()
    {
        IsOpen = false;

        if (vendorUIRoot != null)
        {
            vendorUIRoot.SetActive(false);
        }

        UnlockCursor();
    }

    private void UnlockCursor()
    {
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

        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        UnlockCursor();
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
            Debug.Log("아이템 구매 패널이 연결되어 있지 않아요.");
        }

        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        UnlockCursor();
        SetupItemBuyButtonsAsNotReady();
    }

    private void ShowQuestPanel()
    {
        if (oreSalesPanel != null)
        {
            oreSalesPanel.SetActive(false);
        }

        if (itemBuyPanel != null)
        {
            itemBuyPanel.SetActive(false);
        }

        if (questPanel != null)
        {
            questPanel.SetActive(true);
        }

        UnlockCursor();
        RefreshQuestUI();
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

        button.interactable = true;

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
            Debug.LogError("InventorySystem이 없어요.");
            return;
        }

        Hotbar hotbar = inventorySystem.Hotbar;
        Inventory inventory = inventorySystem.Inventory;

        if (hotbar == null || inventory == null)
        {
            Debug.LogError("Hotbar 또는 Inventory 참조가 없어요.");
            return;
        }

        int totalPrice = CalculateTotalPrice(hotbar, inventory);

        if (totalPrice <= 0)
        {
            Debug.Log("판매할 광석이 없어요.");
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
            Debug.LogError("CoinManager가 없어요.");
        }

        RefreshInventoryUIs();
        RefreshOreSalesUI();

        Debug.Log($"광석 전체 판매 완료: +{totalPrice} Y");
    }

    private void OnClickQuestRewardButton()
    {
        if (QuestManager.instance == null)
        {
            Debug.LogError("QuestManager가 없어요.");
            RefreshQuestUI();
            return;
        }

        bool claimed = QuestManager.instance.TryClaimReward();

        if (!claimed)
        {
            Debug.Log("퀘스트 조건 미달 또는 이미 보상 수령 완료");
        }

        RefreshQuestUI();
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

    private void RefreshQuestUI()
    {
        if (QuestManager.instance == null)
        {
            if (questProgressText != null)
            {
                questProgressText.text = "퀘스트 정보를 불러올 수 없어요.";
            }

            if (questRewardButton != null)
            {
                questRewardButton.interactable = false;
            }

            return;
        }

        if (questProgressText != null)
        {
            if (QuestManager.instance.IsRewardClaimed)
            {
                questProgressText.text =
                    "[튜토리얼 퀘스트]\n\n" +
                    "퀘스트 완료\n" +
                    "보상 수령 완료";
            }
            else
            {
                questProgressText.text =
                    "[튜토리얼 퀘스트]\n\n" +
                    "아무 광물 5개 채굴하기\n" +
                    $"진행도: {QuestManager.instance.CurrentMineCount} / {QuestManager.instance.RequiredMineCount}\n" +
                    $"보상: {QuestManager.instance.RewardCoin} Y";
            }
        }

        if (questRewardButton != null)
        {
            questRewardButton.interactable = QuestManager.instance.CanClaimReward;

            TMP_Text buttonText = questRewardButton.GetComponentInChildren<TMP_Text>(true);

            if (buttonText != null)
            {
                if (QuestManager.instance.IsRewardClaimed)
                {
                    buttonText.text = "완료";
                }
                else if (QuestManager.instance.CanClaimReward)
                {
                    buttonText.text = "보상 받기";
                }
                else
                {
                    buttonText.text = "받기";
                }
            }
        }
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