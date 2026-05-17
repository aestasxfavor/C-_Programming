using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Time")]
    [SerializeField] private float stageTime = 60f;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Unit UI")]
    [SerializeField] private TextMeshProUGUI unitText;

    [Header("Final Boss")]
    [SerializeField] private BossSpawner bossSpawner;
    [SerializeField] private float bossSpawnTime = 10f;

    [Header("References")]
    [SerializeField] private PlayerUnitManager playerUnitManager;
    [SerializeField] private PlayerAttackController playerAttackController;
    [SerializeField] private StageObjectSpawner stageObjectSpawner;

    [Header("Result UI")]
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private GameObject losePanel;

    private float remainingTime;
    private bool isStageEnded;
    private bool hasSpawnedBoss;

    private void Start()
    {
        Time.timeScale = 1f;

        remainingTime = stageTime;
        isStageEnded = false;
        hasSpawnedBoss = false;

        if (winnerPanel != null)
        {
            winnerPanel.SetActive(false);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        if (bossSpawner != null)
        {
            bossSpawner.OnFinalBossDied += HandleFinalBossDied;
        }

        UpdateTimerUI();
        UpdateUnitUI();
    }

    private void OnDestroy()
    {
        if (bossSpawner != null)
        {
            bossSpawner.OnFinalBossDied -= HandleFinalBossDied;
        }
    }

    private void Update()
    {
        if (isStageEnded)
        {
            return;
        }

        CheckFinalBossSpawn();
        UpdateTimer();
        UpdateUnitUI();
        CheckUnitCount();
    }

    private void CheckFinalBossSpawn()
    {
        if (hasSpawnedBoss)
        {
            return;
        }

        // 남은 시간이 기준 시간 이하가 되면 최종 보스를 한 번만 생성
        if (remainingTime > bossSpawnTime)
        {
            return;
        }

        hasSpawnedBoss = true;

        if (bossSpawner != null)
        {
            bossSpawner.SpawnFinalBoss();
        }
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
        {
            return;
        }

        int seconds = Mathf.CeilToInt(remainingTime);
        timerText.text = $"{seconds}";
    }

    private void UpdateUnitUI()
    {
        if (unitText == null)
        {
            return;
        }

        if (playerUnitManager == null)
        {
            return;
        }

        unitText.text = $"Unit : {playerUnitManager.CurrentUnitCount}";
    }

    private void CheckUnitCount()
    {
        if (playerUnitManager == null)
        {
            return;
        }

        if (playerUnitManager.CurrentUnitCount <= 0)
        {
            StageFail();
        }
    }

    private void HandleFinalBossDied()
    {
        StageClear();
    }

    private void StageClear()
    {
        if (isStageEnded)
        {
            return;
        }

        isStageEnded = true;

        StopGameFlow();

        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);
        }

        Debug.Log("Stage Clear");
    }

    private void StageFail()
    {
        if (isStageEnded)
        {
            return;
        }

        isStageEnded = true;

        StopGameFlow();

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }

        Debug.Log("Stage Fail");
    }

    private void StopGameFlow()
    {
        if (stageObjectSpawner != null)
        {
            stageObjectSpawner.enabled = false;
        }

        if (playerAttackController != null)
        {
            playerAttackController.enabled = false;
        }

        Time.timeScale = 0f;
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }
}