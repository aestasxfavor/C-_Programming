using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float xLimit = 4f;
    [SerializeField] private float moveSmoothSpeed = 12f;

    private float targetX;
    private float startZ;

    private void Awake()
    {
        targetX = transform.position.x;
        startZ = transform.position.z;
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
        Vector3 nextPosition = transform.position;

        nextPosition.x = Mathf.Lerp(nextPosition.x, targetX, moveSmoothSpeed * Time.deltaTime);

        // 전진은 스테이지 오브젝트가 움직이므로 플레이어 Z 위치는 고정
        nextPosition.z = startZ;

        transform.position = nextPosition;
    }
}