using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    [Header("Tutorial Quest")]
    [SerializeField] private int requiredMineCount = 5;
    [SerializeField] private int rewardCoin = 500;

    private int currentMineCount;
    private bool isRewardClaimed;

    public int CurrentMineCount => currentMineCount;
    public int RequiredMineCount => requiredMineCount;
    public int RewardCoin => rewardCoin;
    public bool IsRewardClaimed => isRewardClaimed;
    public bool CanClaimReward => currentMineCount >= requiredMineCount && !isRewardClaimed;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMineCount()
    {
        if (isRewardClaimed)
        {
            return;
        }

        currentMineCount++;

        if (currentMineCount > requiredMineCount)
        {
            currentMineCount = requiredMineCount;
        }

        Debug.Log($"퀘스트 진행도: {currentMineCount} / {requiredMineCount}");
    }

    public bool TryClaimReward()
    {
        if (!CanClaimReward)
        {
            return false;
        }

        if (CoinManager.instance == null)
        {
            Debug.LogError("CoinManager가 없습니다.");
            return false;
        }

        CoinManager.instance.AddCoin(rewardCoin);
        isRewardClaimed = true;

        Debug.Log($"퀘스트 완료 보상 지급: +{rewardCoin} Y");
        return true;
    }
}