using UnityEngine;

public class MiniMapCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 20f, 0f);

    [Header("Map Rotation")]
    [SerializeField] private float mapYaw = 90f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(90f, mapYaw, 0f);
    }
}