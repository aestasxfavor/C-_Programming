using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string startSceneName = "Square";

    [Header("Input")]
    [SerializeField] private bool anyKeyToStart = true;

    private bool isLoading;

    private void Update()
    {
        if (isLoading)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (anyKeyToStart && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            StartGame();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ExitGame();
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
}
