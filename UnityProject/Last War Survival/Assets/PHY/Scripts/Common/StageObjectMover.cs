using UnityEngine;

public class StageObjectMover : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float disableZ = -12f;

    private bool canMove = true;

    private void Update()
    {
        if (!canMove)
        {
            return;
        }

        transform.position += Vector3.back * moveSpeed * Time.deltaTime;

        if (transform.position.z <= disableZ)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void StopMove()
    {
        canMove = false;
    }

    public void ResumeMove()
    {
        canMove = true;
    }
}