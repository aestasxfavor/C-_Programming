using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float xLimit = 4f;
    [SerializeField] private float moveSmooth = 12f;

    private float targetX;
    private float fixedZ;

    private void Awake()
    {
        targetX = transform.position.x;
        fixedZ = transform.position.z;
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }
    }

    private void Update()
    {
        UpdateMoveInput();
        MovePlayer();
    }

    private void UpdateMoveInput()
    {
        if (moveAction == null)
        {
            return;
        }

        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

        targetX += moveInput.x * moveSpeed * Time.deltaTime;
        targetX = Mathf.Clamp(targetX, -xLimit, xLimit);
    }

    private void MovePlayer()
    {
        Vector3 currentPosition = transform.position;

        currentPosition.x = Mathf.Lerp(
            currentPosition.x,
            targetX,
            moveSmooth * Time.deltaTime
        );

        currentPosition.z = fixedZ;

        transform.position = currentPosition;
    }
}