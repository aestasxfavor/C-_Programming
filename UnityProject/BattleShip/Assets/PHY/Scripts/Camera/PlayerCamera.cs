using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Direction Target")]
    [SerializeField] private Transform directionTarget;

    [Header("Camera Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 3f, -7f);

    [Header("Look Target")]
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Follow")]
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Transform rotationBase = directionTarget != null ? directionTarget : target;

        Vector3 targetPosition = target.position + rotationBase.rotation * offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        Vector3 lookPosition = target.position + lookOffset;
        Vector3 lookDirection = lookPosition - transform.position;

        if (lookDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}