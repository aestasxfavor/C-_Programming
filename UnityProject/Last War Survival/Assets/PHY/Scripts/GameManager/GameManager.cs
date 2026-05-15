using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Time")]
    [SerializeField] private float playTime = 60f;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("References")]
    [SerializeField] private PlayerUnitManager playerUnitManager;
    [SerializeField] private PlayerAttackController playerAttackController;
    [SerializeField] private StageObjectSpawner stageObjectSpawner;

    [Header("Result UI")]
    [SerializeField] private GameObject successUI;
    [SerializeField] private GameObject failUI;

    private float remainTime;
    private bool isGameEnded;

    private void Start()
    {
        remainTime = playTime;
        isGameEnded = false;

        if (successUI != null)
        {
            successUI.SetActive(false);
        }

        if (failUI != null)
        {
            failUI.SetActive(false);
        }

        UpdateTimerUI();
    }

    private void Update()
    {
        if (isGameEnded)
        {
            return;
        }

        CheckUnitCount();
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        remainTime -= Time.deltaTime;

        if (remainTime <= 0f)
        {
            remainTime = 0f;
            UpdateTimerUI();
            StageClear();
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
        timerText.text = $"Time : {seconds}";
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
    }
}