using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 3f, -7f);

    [Header("Look")]
    [SerializeField] private float lookHeight = 1.5f;

    [Header("Follow")]
    [SerializeField] private float followSpeed = 12f;
    [SerializeField] private float rotateSpeed = 14f;

    private Transform resolvedTarget;

    private void Awake()
    {
        ResolveTarget();
        SnapCamera();
    }

    private void Start()
    {
        ResolveTarget();
        SnapCamera();
    }

    private void LateUpdate()
    {
        ResolveTarget();

        if (resolvedTarget == null)
        {
            return;
        }

        FollowTarget();
        LookAtTarget();
    }

    private void ResolveTarget()
    {
        if (target != null)
        {
            PlayerController controller = target.GetComponent<PlayerController>();

            if (controller == null)
            {
                controller = target.GetComponentInParent<PlayerController>();
            }

            if (controller != null)
            {
                resolvedTarget = controller.transform;
                return;
            }

            resolvedTarget = target;
            return;
        }

        PlayerController foundController = FindFirstObjectByType<PlayerController>();

        if (foundController != null)
        {
            resolvedTarget = foundController.transform;
            target = resolvedTarget;
        }
    }

    private void SnapCamera()
    {
        if (resolvedTarget == null)
        {
            return;
        }

        Vector3 targetPosition = resolvedTarget.position + offset;
        Vector3 lookTarget = resolvedTarget.position + Vector3.up * lookHeight;
        Vector3 lookDirection = lookTarget - targetPosition;

        if (lookDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        transform.position = targetPosition;
        transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }

    private void FollowTarget()
    {
        Vector3 targetPosition = resolvedTarget.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }

    private void LookAtTarget()
    {
        Vector3 lookTarget = resolvedTarget.position + Vector3.up * lookHeight;
        Vector3 lookDirection = lookTarget - transform.position;

        if (lookDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }
}