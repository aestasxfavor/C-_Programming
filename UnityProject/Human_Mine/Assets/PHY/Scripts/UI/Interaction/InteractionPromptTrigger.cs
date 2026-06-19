using UnityEngine;

public class InteractionPromptTrigger : MonoBehaviour
{
    [Header("Prompt")]
    [SerializeField] private string promptMessage = "[F] Ã¤Áý";

    [Header("Layer")]
    [SerializeField] private LayerMask playerLayer;

    private bool isPlayerNear;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsInPlayerLayer(other.gameObject))
        {
            return;
        }

        isPlayerNear = true;

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

        HidePrompt();
    }

    private void OnDisable()
    {
        HidePrompt();
    }

    private void OnDestroy()
    {
        HidePrompt();
    }

    private void HidePrompt()
    {
        if (!isPlayerNear)
        {
            return;
        }

        isPlayerNear = false;

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt(gameObject);
        }
    }

    private bool IsInPlayerLayer(GameObject target)
    {
        return (playerLayer.value & (1 << target.layer)) != 0;
    }
}