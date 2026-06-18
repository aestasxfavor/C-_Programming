using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerFallRespawner : MonoBehaviour
{
    [Header("Fall Check")]
    [SerializeField] private float fallY = -10f;

    [Header("Respawn Point Layer")]
    [SerializeField] private LayerMask respawnPointLayer;

    private Transform respawnPoint;
    private CharacterController characterController;
    private Rigidbody rb;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        FindRespawnPoint();
    }

    private void Update()
    {
        if (transform.position.y <= fallY)
        {
            Respawn();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindRespawnPoint();
    }

    private void FindRespawnPoint()
    {
        respawnPoint = null;

        Transform[] transforms = FindObjectsByType<Transform>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (Transform target in transforms)
        {
            if (IsInLayerMask(target.gameObject.layer, respawnPointLayer))
            {
                respawnPoint = target;
                return;
            }
        }

        Debug.LogWarning("RespawnPoint Layer를 가진 오브젝트를 찾지 못했어요.");
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private void Respawn()
    {
        if (respawnPoint == null)
        {
            FindRespawnPoint();

            if (respawnPoint == null)
            {
                return;
            }
        }

        ResetVelocity();
        MoveToRespawnPoint();
    }

    private void ResetVelocity()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void MoveToRespawnPoint()
    {
        if (characterController != null)
        {
            characterController.enabled = false;
            transform.position = respawnPoint.position;
            characterController.enabled = true;
            return;
        }

        transform.position = respawnPoint.position;
    }
}