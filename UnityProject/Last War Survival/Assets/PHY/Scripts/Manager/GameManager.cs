using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Time")]
    [SerializeField] private float playTime = 60f;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Unit UI")]
    [SerializeField] private TextMeshProUGUI unitText;

    [Header("Final Boss")]
    [SerializeField] private BossSpawner bossSpawner;
    [SerializeField] private float bossSpawnRemainTime = 10f;

    [Header("References")]
    [SerializeField] private PlayerUnitManager playerUnitManager;
    [SerializeField] private PlayerAttackController playerAttackController;
    [SerializeField] private StageObjectSpawner stageObjectSpawner;

    [Header("Result UI")]
    [SerializeField] private GameObject successUI;
    [SerializeField] private GameObject failUI;

    private float remainTime;
    private bool isGameEnded;
    private bool hasRequestedFinalBoss;

    private void Start()
    {
        Time.timeScale = 1f;

        remainTime = playTime;
        isGameEnded = false;
        hasRequestedFinalBoss = false;

        if (successUI != null)
        {
            successUI.SetActive(false);
        }

        if (failUI != null)
        {
            failUI.SetActive(false);
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
        if (isGameEnded)
        {
            return;
        }

        CheckUnitCount();
        CheckFinalBossSpawn();
        UpdateTimer();
        UpdateUnitUI();
    }

    private void CheckFinalBossSpawn()
    {
        if (hasRequestedFinalBoss)
        {
            return;
        }

        if (remainTime > bossSpawnRemainTime)
        {
            return;
        }

        hasRequestedFinalBoss = true;

        if (bossSpawner != null)
        {
            bossSpawner.SpawnFinalBoss();
        }
        else
        {
            Debug.LogWarning("FinalBossSpawner가 GameManager에 연결되지 않았어요.");
        }
    }

    private void UpdateTimer()
    {
        remainTime -= Time.deltaTime;

        if (remainTime <= 0f)
        {
            remainTime = 0f;
            UpdateTimerUI();
            return;
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
        {
            return;
        }

        int seconds = Mathf.CeilToInt(remainTime);
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
        Debug.Log("Final Boss Clear");

        StageClear();
    }

    private void StageClear()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        StopGameFlow();

        if (successUI != null)
        {
            successUI.SetActive(true);
        }

        Debug.Log("Stage Clear");
    }

    private void StageFail()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        StopGameFlow();

        if (failUI != null)
        {
            failUI.SetActive(true);
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