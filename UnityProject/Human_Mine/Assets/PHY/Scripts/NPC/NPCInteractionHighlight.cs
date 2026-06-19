using UnityEngine;

public class NPCInteractionHighlight : MonoBehaviour
{
    [Header("Detect")]
    [SerializeField] private float detectRadius = 2.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Highlight")]
    [SerializeField] private GameObject highlightObject;

    [Header("Prompt")]
    [SerializeField] private string promptMessage = "[Shift] 상점 열기";

    private bool isPlayerNear;

    private void OnEnable()
    {
        isPlayerNear = false;
        SetHighlight(false);

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt(gameObject);
        }
    }

    private void Start()
    {
        isPlayerNear = false;
        SetHighlight(false);

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt(gameObject);
        }
    }

    private void Update()
    {
        CheckPlayerNear();
    }

    private void CheckPlayerNear()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectRadius,
            playerLayer,
            QueryTriggerInteraction.Collide
        );

        bool foundPlayer = hits.Length > 0;

        if (foundPlayer)
        {
            isPlayerNear = true;
            SetHighlight(true);

            if (IsVendorUIOpen())
            {
                HidePrompt();
            }
            else
            {
                ShowPrompt();
            }

            return;
        }

        if (!isPlayerNear)
        {
            return;
        }

        isPlayerNear = false;
        SetInteractionState(false);
    }

    private bool IsVendorUIOpen()
    {
        return VendorUIController.instance != null && VendorUIController.instance.IsOpen;
    }

    private void SetInteractionState(bool active)
    {
        SetHighlight(active);

        if (active)
        {
            ShowPrompt();
        }
        else
        {
            HidePrompt();
        }
    }

    private void SetHighlight(bool active)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(active);
        }
    }

    private void ShowPrompt()
    {
        if (InteractionPromptUI.Instance == null)
        {
            return;
        }

        InteractionPromptUI.Instance.ShowPrompt(promptMessage, gameObject);
    }

    private void HidePrompt()
    {
        if (InteractionPromptUI.Instance == null)
        {
            return;
        }

        InteractionPromptUI.Instance.HidePrompt(gameObject);
    }

    private void OnDisable()
    {
        isPlayerNear = false;
        SetHighlight(false);

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}