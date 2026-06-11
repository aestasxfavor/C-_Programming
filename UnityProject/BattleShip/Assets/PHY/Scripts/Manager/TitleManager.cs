using UnityEngine;
using UnityEngine.SceneManagement;

// 타이틀 화면의 메인 메뉴, 모드 선택, 싱글 안내, 멀티 씬 이동을 담당하는 UI 매니저
public class TitleManager : MonoBehaviour
{
    [Header("타이틀 메인 패널")]
    [SerializeField] private GameObject mainButtonPanel;

    [Header("모드 선택 패널")]
    [SerializeField] private GameObject modeSelectPanel;

    [Header("싱글 준비 중 안내")]
    [SerializeField] private GameObject singleReadyPanel;

    private void Start()
    {
        ShowMainPanel();
    }

    public void OnClickStartButton()
    {
        ShowModeSelectPanel();
    }

    public void OnClickSingleButton()
    {
        if (singleReadyPanel != null)
        {
            singleReadyPanel.SetActive(true);
        }

        Debug.Log("[Title] 싱글 플레이는 준비 중");
    }

    public void OnClickMultiButton()
    {
        SceneManager.LoadScene("Game");
    }

    public void OnClickBackButton()
    {
        ShowMainPanel();
    }

    public void OnClickExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowMainPanel()
    {
        if (mainButtonPanel != null)
        {
            mainButtonPanel.SetActive(true);
        }

        if (modeSelectPanel != null)
        {
            modeSelectPanel.SetActive(false);
        }

        if (singleReadyPanel != null)
        {
            singleReadyPanel.SetActive(false);
        }
    }

    private void ShowModeSelectPanel()
    {
        if (mainButtonPanel != null)
        {
            mainButtonPanel.SetActive(false);
        }

        if (modeSelectPanel != null)
        {
            modeSelectPanel.SetActive(true);
        }

        if (singleReadyPanel != null)
        {
            singleReadyPanel.SetActive(false);
        }
    }
}