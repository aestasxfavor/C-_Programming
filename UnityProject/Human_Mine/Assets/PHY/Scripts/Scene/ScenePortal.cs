using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ScenePortal : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string targetSceneName;

    [Header("Interaction")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private string promptMessage = "[E] 이동";

    [Header("Detect")]
    [SerializeField] private LayerMask playerLayer;

    private bool isPlayerInRange;
    private bool isLoading;
    private bool enabledInteractActionHere;

    private void OnEnable()
    {
        if (interactAction != null &&
            interactAction.action != null &&
            !interactAction.action.enabled)
        {
            interactAction.action.Enable();
            enabledInteractActionHere = true;
        }
    }

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

        if (interactAction == null || interactAction.action == null)
        {
            return;
        }

        if (interactAction.action.WasPressedThisFrame())
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

        if (interactAction != null &&
            interactAction.action != null &&
            enabledInteractActionHere)
        {
            interactAction.action.Disable();
            enabledInteractActionHere = false;
        }
    }
}