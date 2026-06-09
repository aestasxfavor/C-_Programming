using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIController : MonoBehaviour
{
    [Header("배치 / 전투 UI")]
    [SerializeField] private GameObject placementShipPanel;
    [SerializeField] private GameObject enemyBoardPanel;

    [Header("게임 종료 UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("연결 끊김 UI")]
    [SerializeField] private GameObject disconnectPanel;

    [Header("상태 텍스트")]
    [SerializeField] private TextMeshProUGUI gameStatusText;
    [SerializeField] private TextMeshProUGUI turnTimeText;

    [Header("함선 상태 UI")]
    [SerializeField] private GameObject shipStatusPanel;
    [SerializeField] private Image[] myShipStatusIcons;
    [SerializeField] private Image[] enemyShipStatusIcons;
    [SerializeField] private Color aliveShipColor = Color.white;
    [SerializeField] private Color sunkShipColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    public void ShowPlacementUI()
    {
        if (placementShipPanel != null)
        {
            placementShipPanel.SetActive(true);
        }

        if (enemyBoardPanel != null)
        {
            enemyBoardPanel.SetActive(false);
        }

        SetShipStatusPanelVisible(false);
    }

    public void ShowBattleUI()
    {
        if (placementShipPanel != null)
        {
            placementShipPanel.SetActive(false);
        }

        if (enemyBoardPanel != null)
        {
            enemyBoardPanel.SetActive(true);
        }

        SetShipStatusPanelVisible(true);
    }

    public void HideGameOverUI()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }
    }

    public void ShowGameOverUI(bool isWin)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(isWin);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(!isWin);
        }
    }

    public void ShowDisconnectPanel()
    {
        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(true);
        }
    }

    public void HideDisconnectPanel()
    {
        if (disconnectPanel != null)
        {
            disconnectPanel.SetActive(false);
        }
    }

    public void SetStatusText(string text)
    {
        if (gameStatusText == null)
        {
            return;
        }

        gameStatusText.text = text;
    }

    public void SetTurnTimeText(int seconds)
    {
        if (turnTimeText == null)
        {
            return;
        }

        turnTimeText.text = $"남은 시간: {seconds}초";
    }

    public void ClearTurnTimeText()
    {
        if (turnTimeText == null)
        {
            return;
        }

        turnTimeText.text = "";
    }

    public void ResetShipStatus()
    {
        ResetShipIconColors(myShipStatusIcons);
        ResetShipIconColors(enemyShipStatusIcons);
    }

    public void MarkMyShipSunk(string shipId)
    {
        MarkShipStatusIconSunk(myShipStatusIcons, shipId);
    }

    public void MarkEnemyShipSunk(string shipId)
    {
        MarkShipStatusIconSunk(enemyShipStatusIcons, shipId);
    }

    private void ResetShipIconColors(Image[] shipIcons)
    {
        if (shipIcons == null)
        {
            return;
        }

        for (int i = 0; i < shipIcons.Length; i++)
        {
            if (shipIcons[i] != null)
            {
                shipIcons[i].color = aliveShipColor;
            }
        }
    }

    private void MarkShipStatusIconSunk(Image[] shipIcons, string shipId)
    {
        int iconIndex = GetShipStatusIconIndex(shipId);

        if (iconIndex < 0 || shipIcons == null || iconIndex >= shipIcons.Length)
        {
            return;
        }

        if (shipIcons[iconIndex] != null)
        {
            shipIcons[iconIndex].color = sunkShipColor;
        }
    }

    private int GetShipStatusIconIndex(string shipId)
    {
        switch (shipId)
        {
            case "Ship2":
                return 0;

            case "Ship3A":
                return 1;

            case "Ship3B":
                return 2;

            case "Ship4":
                return 3;

            case "Ship5":
                return 4;

            default:
                return -1;
        }
    }

    private void SetShipStatusPanelVisible(bool isVisible)
    {
        if (shipStatusPanel == null)
        {
            return;
        }

        shipStatusPanel.SetActive(true);

        for (int i = 0; i < shipStatusPanel.transform.childCount; i++)
        {
            Transform child = shipStatusPanel.transform.GetChild(i);
            SetChildObjectsVisible(child, isVisible);
        }
    }

    private void SetChildObjectsVisible(Transform target, bool isVisible)
    {
        if (target == null)
        {
            return;
        }

        target.gameObject.SetActive(isVisible);

        for (int i = 0; i < target.childCount; i++)
        {
            SetChildObjectsVisible(target.GetChild(i), isVisible);
        }
    }
}