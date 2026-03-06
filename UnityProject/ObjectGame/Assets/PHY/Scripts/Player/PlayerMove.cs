using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameObject GameOverPanel;

    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI timeText;

    private float score = 0f;
    private float startTime;

    private void Start()
    {
        startTime = Time.time;
    }
    void Update()
    {
        float survivalTime = Time.time - startTime;
        timeText.text = "Time: " + Mathf.FloorToInt(survivalTime);

        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            y = 1f;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            y = -1f;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            x = -1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            x = 1f;
        }

        Vector2 dir = new Vector2(x, y).normalized;     //속도 정규화
        transform.Translate(dir * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Time.timeScale = 0f;
            GameOverPanel.SetActive(true);
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Score"))
        {
            score += 5;
            UpdateScoreUI();
            collision.gameObject.SetActive(false);
        }
    }

    public void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    public void AddScore(float _score)
    {
        score += _score;
        UpdateScoreUI();
    }
}
