using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text endScoreText;

    [Header("Game Setting")]
    [SerializeField] private float gameDuration = 30f;
    [SerializeField] private MoleSpawner moleSpawner;

    private int currentScore = 0;
    private int bestScore = 0;
    private float currentTime;
    private bool isGamePlaying = false;

    private const string BestScoreKey = "MoleBestScore";

    public bool IsGamePlaying => isGamePlaying;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);

        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }

        StartGame();
    }

    private void Update()
    {
        if (!isGamePlaying) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            EndGame();
        }

        UpdateUI();
    }

    public void AddScore(int amount)
    {
        if (!isGamePlaying) return;

        currentScore += amount;

        if (currentScore < 0)
        {
            currentScore = 0;
        }

        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
            PlayerPrefs.Save();
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        bestScoreText.text = $"BestScore : {bestScore}";
        scoreText.text = $"Score : {currentScore}";
        timeText.text = $"{Mathf.CeilToInt(currentTime)}";
    }

    public void StartGame()
    {
        currentScore = 0;
        currentTime = gameDuration;
        isGamePlaying = true;

        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }

        if (moleSpawner != null)
        {
            moleSpawner.StartSpawning();
        }

        UpdateUI();
    }

    public void EndGame()
    {
        isGamePlaying = false;

        if (moleSpawner != null)
        {
            moleSpawner.StopSpawning();
        }

        if (endPanel != null)
        {
            endPanel.SetActive(true);
        }

        if (endScoreText != null)
        {
            endScoreText.text = $"Final Score : {currentScore}";
        }

        UpdateUI();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene("Title");
    }
}