using UnityEngine;
using UnityEngine.InputSystem;
using InventoryFramework;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference miningAction;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask interactMask = ~0;

    [Header("Sound")]
    [SerializeField] private MiningSound miningSound;

    private ItemPickupHandler pickupHandler;
    private MineableOre currentMiningOre;
    private bool enabledMiningActionHere;

    private void Awake()
    {
        pickupHandler = GetComponent<ItemPickupHandler>();

        if (miningSound == null)
        {
            miningSound = GetComponent<MiningSound>();
        }
    }

    private void OnEnable()
    {
        if (miningAction != null &&
            miningAction.action != null &&
            !miningAction.action.enabled)
        {
            miningAction.action.Enable();
            enabledMiningActionHere = true;
        }
    }

    private void Update()
    {
        if (miningAction == null || miningAction.action == null)
        {
            return;
        }

        if (miningAction.action.WasPressedThisFrame())
        {
            TryStartMining();
        }

        if (miningAction.action.WasReleasedThisFrame())
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

        if (miningSound != null)
        {
            miningSound.StartMiningSound();
        }
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

        if (miningSound != null)
        {
            miningSound.StopMiningSound();
        }
    }

    private void OnDisable()
    {
        CancelCurrentMining();

        if (miningAction != null &&
            miningAction.action != null &&
            enabledMiningActionHere)
        {
            miningAction.action.Disable();
            enabledMiningActionHere = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}