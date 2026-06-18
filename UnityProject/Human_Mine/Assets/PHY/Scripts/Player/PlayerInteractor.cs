using UnityEngine;
using UnityEngine.InputSystem;
using InventoryFramework;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask interactMask = ~0;

    private ItemPickupHandler pickupHandler;
    private MineableOre currentMiningOre;

    private void Awake()
    {
        pickupHandler = GetComponent<ItemPickupHandler>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            TryStartMining();
        }

        if (Keyboard.current.fKey.wasReleasedThisFrame)
        {
            CancelCurrentMining();
        }
    }

    private void TryStartMining()
    {
        MineableOre nearestOre = FindNearestOre();

        if (nearestOre == null)
        {
            return;
        }

        currentMiningOre = nearestOre;
        currentMiningOre.StartMining(pickupHandler);
    }

    private MineableOre FindNearestOre()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactDistance,
            interactMask,
            QueryTriggerInteraction.Collide
        );

        MineableOre nearestOre = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            MineableOre ore = hit.GetComponentInParent<MineableOre>();

            if (ore == null || ore.IsMined)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, ore.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestOre = ore;
            }
        }

        return nearestOre;
    }

    private void CancelCurrentMining()
    {
        if (currentMiningOre == null)
        {
            return;
        }

        currentMiningOre.CancelMining();
        currentMiningOre = null;
    }

    private void OnDisable()
    {
        CancelCurrentMining();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}