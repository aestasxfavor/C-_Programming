using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump / Gravity")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedVelocity = -2f;

    private CharacterController characterController;
    private Vector2 moveInput;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.Disable();
        }
    }

    private void Update()
    {
        ReadInput();
        Move();
    }

    private void ReadInput()
    {
        if (moveAction == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = moveAction.action.ReadValue<Vector2>();
    }

    private void Move()
    {
        Vector3 moveDirection = GetCameraBasedMoveDirection();

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedVelocity;
        }

        bool jumpPressed = jumpAction != null && jumpAction.action.WasPressedThisFrame();

        if (jumpPressed && characterController.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 horizontalMove = moveDirection * moveSpeed;
        Vector3 verticalMove = Vector3.up * verticalVelocity;

        characterController.Move((horizontalMove + verticalMove) * Time.deltaTime);
    }

    private Vector3 GetCameraBasedMoveDirection()
    {
        if (cameraTransform == null)
        {
            Vector3 fallbackMove = new Vector3(moveInput.x, 0f, moveInput.y);

            if (fallbackMove.sqrMagnitude > 1f)
            {
                fallbackMove.Normalize();
            }

            return fallbackMove;
        }

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        if (cameraForward.sqrMagnitude < 0.01f)
        {
            cameraForward = Vector3.forward;
        }

        if (cameraRight.sqrMagnitude < 0.01f)
        {
            cameraRight = Vector3.right;
        }

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection =
            cameraForward * moveInput.y +
            cameraRight * moveInput.x;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        return moveDirection;
    }
}