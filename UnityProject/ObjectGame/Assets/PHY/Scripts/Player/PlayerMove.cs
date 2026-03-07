using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private Life lifeScript;

    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI timeText;

    private float score = 0f;
    private float startTime;

    private bool isInvincible = false;

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

        Vector2 dir = new Vector2(x, y).normalized;    
        transform.Translate(dir * moveSpeed * Time.deltaTime);  //속도 정규화
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvincible) return;     // 무적 상태면 충돌 무시

        // 장애물 충돌 → 라이프 감소
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            lifeScript.TakeDamage();      // UI 업데이트 + 라이프 감소 처리

            if (GameOverPanel.activeSelf == false)
            {
                // 수명 남아있으면 플레이어만 리스폰
                transform.position = Vector3.zero;
                StartCoroutine(InvincibleRoutine());
            }
        }

        // 점수 획득
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

    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        float time = 0f;
        while (time < 1f)
        {
            sprite.enabled = !sprite.enabled;   // 깜빡
            yield return new WaitForSeconds(0.1f);
            time += 0.1f;
        }

        sprite.enabled = true;  // 다시 보이게
        isInvincible = false;
    }
}
