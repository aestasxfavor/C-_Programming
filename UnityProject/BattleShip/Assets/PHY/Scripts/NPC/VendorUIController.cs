using UnityEngine;
using UnityEngine.UI;

public class VendorUIController : MonoBehaviour
{
    public static VendorUIController instance;

    [Header("Root")]
    [SerializeField] private GameObject vendorUIRoot;

    [Header("Panels")]
    [SerializeField] private GameObject oreSalesPanel;
    [SerializeField] private GameObject itemBuyPanel;

    [Header("Buttons")]
    [SerializeField] private Button mineralSalesButton;
    [SerializeField] private Button itemBuyButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button sellButton;

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
    }

    private void ShowItemBuyPanel()
    {
        if (itemBuyPanel == null)
        {
            Debug.Log("아이템 구매 기능은 준비중입니다.");
            return;
        }

        if (oreSalesPanel != null)
        {
            oreSalesPanel.SetActive(false);
        }

        itemBuyPanel.SetActive(true);
    }

    private void OnClickSellButton()
    {
        Debug.Log("전체 판매 버튼 클릭");
    }
}