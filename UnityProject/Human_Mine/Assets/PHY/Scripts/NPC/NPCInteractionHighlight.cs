using UnityEngine;

public class NPCInteractionHighlight : MonoBehaviour
{
    [Header("Detect")]
    [SerializeField] private float detectRadius = 2.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Highlight")]
    [SerializeField] private GameObject highlightObject;

    private bool isPlayerNear;

    private void Start()
    {
        SetHighlight(false);
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
        {
            return;
        }

        isPlayerNear = foundPlayer;
        SetHighlight(isPlayerNear);
    }

    private void SetHighlight(bool active)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(active);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}