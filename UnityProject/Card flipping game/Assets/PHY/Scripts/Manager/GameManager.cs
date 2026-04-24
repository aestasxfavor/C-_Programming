using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform cardRoot;
    [SerializeField] private Sprite[] cardSprites;

    [Header("InGame UI")]
    [SerializeField] private TMP_Text scoreText;

    [Header("End UI")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text finalScoreText;

    private Card firstCard;
    private Card secondCard;

    private List<Card> spawnedCards = new List<Card>();

    private int score;
    private int flipCount;
    private int matchedCount;

    private bool isChecking;

    private void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }

        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }

        CreateCards();
        UpdateUI();
        StartCoroutine(StartPreviewRoutine());
    }

    // 카드 생성 로직
    private void CreateCards()
    {
        ClearCards();
        spawnedCards.Clear();

        score = 0;
        flipCount = 0;
        matchedCount = 0;

        firstCard = null;
        secondCard = null;
        isChecking = true;

        List<int> cardIds = new List<int>();

        for (int i = 0; i < cardSprites.Length; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        Shuffle(cardIds);

        for (int i = 0; i < cardIds.Count; i++)
        {
            int id = cardIds[i];

            Card card = Instantiate(cardPrefab, cardRoot);
            card.Init(id, cardSprites[id], this);

            spawnedCards.Add(card);
        }
    }

    // 시작할 때 카드 앞면 잠깐 보여주기
    private IEnumerator StartPreviewRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < spawnedCards.Count; i++)
        {
            spawnedCards[i].OpenCard();
        }

        yield return new WaitForSeconds(0.8f);

        for (int i = 0; i < spawnedCards.Count; i++)
        {
            spawnedCards[i].ShowBack();
        }

        yield return new WaitForSeconds(0.4f);

        isChecking = false;
    }

    private void ClearCards()
    {
        for (int i = cardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(cardRoot.GetChild(i).gameObject);
        }
    }

    // 카드 셔플 로직
    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void SelectCard(Card card)
    {
        if (isChecking) return;

        card.ShowFront();
        flipCount++;

        if (firstCard == null)
        {
            firstCard = card;
            UpdateUI();
            return;
        }

        secondCard = card;
        UpdateUI();

        StartCoroutine(CheckMatch());
    }

    private IEnumerator CheckMatch()
    {
        isChecking = true;

        yield return new WaitForSeconds(0.9f);

        if (firstCard.CardId == secondCard.CardId)
        {
            firstCard.SetMatched();
            secondCard.SetMatched();

            firstCard.PlayMatchEffect();
            secondCard.PlayMatchEffect();

            matchedCount += 2;
            score += 5;
        }
        else
        {
            firstCard.ShowBack();
            secondCard.ShowBack();
        }

        firstCard = null;
        secondCard = null;

        UpdateUI();

        if (matchedCount >= cardSprites.Length * 2)
        {
            EndGame();
        }
        else
        {
            yield return new WaitForSeconds(0.35f);
            isChecking = false;
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    private void EndGame()
    {
        if (endPanel != null)
        {
            endPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Game Clear!\nFinal Score: {score}\nFlip Count: {flipCount}";
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ExitGame()
    {
        SceneManager.LoadScene("Title");
    }
}