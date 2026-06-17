using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Visual Rotation")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float modelYawOffset = 0f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animationParameterName = "AnimationPar";
    [SerializeField] private int idleValue = 0;
    [SerializeField] private int runValue = 1;

    private CharacterController characterController;
    private float verticalVelocity;
    private int animationParameterHash;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        ResolveCamera();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        animationParameterHash = Animator.StringToHash(animationParameterName);
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
        ResolveCamera();

        if (IsUIOpen())
        {
            StopPlayerInputWhileUIOpen();
            return;
        }

        Vector2 input = GetMoveInput();
        Vector3 moveDirection = GetMoveDirection(input);

        ApplyJumpAndGravity();
        Move(moveDirection);
        RotateVisual(moveDirection);
        UpdateAnimation(input);
    }

    private bool IsUIOpen()
    {
        return VendorUIController.instance != null && VendorUIController.instance.IsOpen;
    }

    private void StopPlayerInputWhileUIOpen()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        UpdateAnimation(Vector2.zero);
    }

    private void ResolveCamera()
    {
        if (cameraTransform != null)
        {
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
        }
    }

    private Vector2 GetMoveInput()
    {
        if (moveAction == null)
        {
            return Vector2.zero;
        }

        return moveAction.action.ReadValue<Vector2>();
    }

    private Vector3 GetMoveDirection(Vector2 input)
    {
        if (cameraTransform == null)
        {
            return Vector3.zero;
        }

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * input.y + right * input.x;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        return moveDirection;
    }

    private void Move(Vector3 moveDirection)
    {
        Vector3 horizontalMove = moveDirection * moveSpeed;
        Vector3 verticalMove = Vector3.up * verticalVelocity;

        characterController.Move((horizontalMove + verticalMove) * Time.deltaTime);
    }

    private void RotateVisual(Vector3 moveDirection)
    {
        if (visualRoot == null)
        {
            return;
        }

        if (moveDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        targetRotation *= Quaternion.Euler(0f, modelYawOffset, 0f);

        visualRoot.rotation = Quaternion.RotateTowards(
            visualRoot.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void ApplyJumpAndGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        bool jumpPressed = jumpAction != null && jumpAction.action.WasPressedThisFrame();

        if (characterController.isGrounded && jumpPressed)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void UpdateAnimation(Vector2 input)
    {
        if (animator == null)
        {
            return;
        }

        bool isMoving = input.sqrMagnitude > 0.01f;
        animator.SetInteger(animationParameterHash, isMoving ? runValue : idleValue);
    }
}