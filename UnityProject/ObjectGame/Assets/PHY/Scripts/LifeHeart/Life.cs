using UnityEngine;
using UnityEngine.UI;

public class Life : MonoBehaviour
{
    [SerializeField] private int life = 3;
    [SerializeField] private Image[] hearts;
    [SerializeField] private GameObject gameOverPanel;

    public void TakeDamage()
    {
        life--;
        UpdateHearts();

        if (life <= 0)
        {
            // 게임오버 UI 띄우기
            Time.timeScale = 0f;
            gameOverPanel.SetActive(true);
        }
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < life;
        }
    }

    public void ResetLife()
    {
        life = 3;
        UpdateHearts();
    }
}