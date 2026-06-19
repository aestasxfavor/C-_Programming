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

    private void Start()
    {
        SetInteractionState(false);
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

        if (isPlayerNear == foundPlayer)
            return;

        isPlayerNear = foundPlayer;
        SetInteractionState(isPlayerNear);
    }

    private void SetInteractionState(bool active)
    {
        SetHighlight(active);
        SetPrompt(active);
    }

    private void SetHighlight(bool active)
    {
        if (highlightObject != null)
            highlightObject.SetActive(active);
    }

    private void SetPrompt(bool active)
    {
        if (InteractionPromptUI.Instance == null)
            return;

        if (active)
            InteractionPromptUI.Instance.ShowPrompt(promptMessage, gameObject);
        else
            InteractionPromptUI.Instance.HidePrompt(gameObject);
    }

    private void OnDisable()
    {
        if (InteractionPromptUI.Instance != null)
            InteractionPromptUI.Instance.HidePrompt(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}