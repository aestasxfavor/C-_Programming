using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string startSceneName = "Square";

    [Header("Input")]
    [SerializeField] private bool anyKeyToStart = true;
    [SerializeField] private InputActionReference startAction;
    [SerializeField] private InputActionReference exitAction;

    private bool isLoading;
    private bool enabledStartActionHere;
    private bool enabledExitActionHere;

    private void OnEnable()
    {
        if (startAction != null &&
            startAction.action != null &&
            !startAction.action.enabled)
        {
            startAction.action.Enable();
            enabledStartActionHere = true;
        }

        if (exitAction != null &&
            exitAction.action != null &&
            !exitAction.action.enabled)
        {
            exitAction.action.Enable();
            enabledExitActionHere = true;
        }
    }

    private void Update()
    {
        if (isLoading)
        {
            return;
        }

        if (exitAction != null &&
            exitAction.action != null &&
            exitAction.action.WasPressedThisFrame())
        {
            ExitGame();
            return;
        }

        if (!anyKeyToStart)
        {
            return;
        }

        if (startAction != null &&
            startAction.action != null &&
            startAction.action.WasPressedThisFrame())
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        isLoading = true;
        SceneManager.LoadScene(startSceneName);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDisable()
    {
        if (startAction != null &&
            startAction.action != null &&
            enabledStartActionHere)
        {
            startAction.action.Disable();
            enabledStartActionHere = false;
        }

        if (exitAction != null &&
            exitAction.action != null &&
            enabledExitActionHere)
        {
            exitAction.action.Disable();
            enabledExitActionHere = false;
        }
    }
}