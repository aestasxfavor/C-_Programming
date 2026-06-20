using UnityEngine;
using UnityEngine.UI;

public class LobbyUIController : MonoBehaviour
{
    [Header("Guide")]
    [SerializeField] private Button guideButton;
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private Button guideCloseButton;

    [Header("Quit")]
    [SerializeField] private Button quitButton;

    [Header("Quit Confirm")]
    [SerializeField] private GameObject quitConfirmPanel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private void Awake()
    {
        if (guideButton != null)
        {
            guideButton.onClick.AddListener(OpenGuidePanel);
        }

        if (guideCloseButton != null)
        {
            guideCloseButton.onClick.AddListener(CloseGuidePanel);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OpenQuitConfirmPanel);
        }

        if (yesButton != null)
        {
            yesButton.onClick.AddListener(QuitGame);
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(CloseQuitConfirmPanel);
        }
    }

    private void OnEnable()
    {
        UnlockCursor();
        CloseGuidePanel();
        CloseQuitConfirmPanel();
    }

    private void Start()
    {
        UnlockCursor();
        CloseGuidePanel();
        CloseQuitConfirmPanel();
    }

    private void OnDestroy()
    {
        if (guideButton != null)
        {
            guideButton.onClick.RemoveListener(OpenGuidePanel);
        }

        if (guideCloseButton != null)
        {
            guideCloseButton.onClick.RemoveListener(CloseGuidePanel);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OpenQuitConfirmPanel);
        }

        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(QuitGame);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveListener(CloseQuitConfirmPanel);
        }
    }

    private void OpenGuidePanel()
    {
        UnlockCursor();

        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
        }

        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(false);
        }
    }

    private void CloseGuidePanel()
    {
        UnlockCursor();

        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }
    }

    private void OpenQuitConfirmPanel()
    {
        UnlockCursor();

        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(true);
        }

        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }
    }

    private void CloseQuitConfirmPanel()
    {
        UnlockCursor();

        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(false);
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void QuitGame()
    {
        Debug.Log("게임 종료 확인");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}