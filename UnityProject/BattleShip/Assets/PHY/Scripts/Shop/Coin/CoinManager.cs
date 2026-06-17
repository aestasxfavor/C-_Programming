using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;

    [SerializeField] private int coin;
    [SerializeField] private TMP_Text coinText;

    public int Coin => coin;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        RefreshUI();
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        coin += amount;
        RefreshUI();
    }

    public bool SpendCoin(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (coin < amount)
        {
            return false;
        }

        coin -= amount;
        RefreshUI();
        return true;
    }

    public void SetCoinText(TMP_Text text)
    {
        coinText = text;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (coinText != null)
        {
            coinText.text = coin.ToString();
        }
    }
}