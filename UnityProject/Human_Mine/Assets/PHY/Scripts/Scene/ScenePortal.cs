using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ScenePortal : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string targetSceneName;

    [Header("Interaction")]
    [SerializeField] private string promptMessage = "[E] 이동";

    [Header("Detect")]
    [SerializeField] private LayerMask playerLayer;

    private bool isPlayerInRange;
    private bool isLoading;

    private void Update()
    {
        if (!isPlayerInRange)
        {
            return;
        }

        if (isLoading)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            LoadTargetScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading)
        {
            return;
        }

        if (!IsInPlayerLayer(other.gameObject))
        {
            return;
        }

        isPlayerInRange = true;

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.ShowPrompt(promptMessage, gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsInPlayerLayer(other.gameObject))
        {
            return;
        }

        isPlayerInRange = false;

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt(gameObject);
        }
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("ScenePortal: 이동할 씬 이름이 비어 있어요.");
            return;
        }

        isLoading = true;
        isPlayerInRange = false;

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt(gameObject);
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private bool IsInPlayerLayer(GameObject target)
    {
        return ((1 << target.layer) & playerLayer) != 0;
    }

    private void OnDisable()
    {
        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt(gameObject);
        }
    }
}