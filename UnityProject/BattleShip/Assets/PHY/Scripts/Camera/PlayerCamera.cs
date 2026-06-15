using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Third Person View")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -12f);
    [SerializeField] private float lookHeight = 1.6f;

    [Header("Smooth")]
    [SerializeField] private float followSmoothTime = 0.08f;
    [SerializeField] private float rotationSmoothSpeed = 10f;

    private Vector3 currentVelocity;

    private void Start()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.position + offset;
        LookAtTargetInstant();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            followSmoothTime
        );

        RotateToTarget();
    }

    private void RotateToTarget()
    {
        Vector3 lookTarget = target.position + Vector3.up * lookHeight;
        Vector3 lookDirection = lookTarget - transform.position;

        if (lookDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSmoothSpeed * Time.deltaTime
        );
    }

    private void LookAtTargetInstant()
    {
        Vector3 lookTarget = target.position + Vector3.up * lookHeight;
        Vector3 lookDirection = lookTarget - transform.position;

        if (lookDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }
}