using System.Collections;
using UnityEngine;
using InventoryFramework;

public class MineableOre : MonoBehaviour
{
    [Header("Ore Item")]
    [SerializeField] private Item oreItem;
    [SerializeField] private int amount = 1;

    [Header("Mining")]
    [SerializeField] private float mineDuration = 1.5f;
    [SerializeField] private float cancelDistance = 3f;

    private bool isMined;
    private bool isMining;
    private Coroutine miningCoroutine;

    public bool IsMined => isMined;

    private void OnEnable()
    {
        isMined = false;
        isMining = false;
        miningCoroutine = null;
    }

    private void OnDisable()
    {
        CancelMining();
    }

    public void Mine(ItemPickupHandler pickupHandler)
    {
        StartMining(pickupHandler);
    }

    public void StartMining(ItemPickupHandler pickupHandler)
    {
        if (isMined || isMining)
        {
            return;
        }

        if (pickupHandler == null)
        {
            Debug.LogWarning("ItemPickupHandler is missing on player.");
            return;
        }

        if (oreItem == null)
        {
            Debug.LogWarning("Ore item is not assigned.");
            return;
        }

        miningCoroutine = StartCoroutine(MiningRoutine(pickupHandler));
    }

    public void CancelMining()
    {
        if (!isMining && miningCoroutine == null)
        {
            return;
        }

        if (miningCoroutine != null)
        {
            StopCoroutine(miningCoroutine);
            miningCoroutine = null;
        }

        isMining = false;

        if (MiningGaugeUI.instance != null)
        {
            MiningGaugeUI.instance.Hide();
        }
    }

    private IEnumerator MiningRoutine(ItemPickupHandler pickupHandler)
    {
        isMining = true;

        if (MiningGaugeUI.instance != null)
        {
            MiningGaugeUI.instance.Show();
        }

        float timer = 0f;

        while (timer < mineDuration)
        {
            if (pickupHandler == null)
            {
                CancelMining();
                yield break;
            }

            float distance = Vector3.Distance(transform.position, pickupHandler.transform.position);

            if (distance > cancelDistance)
            {
                CancelMining();
                yield break;
            }

            timer += Time.deltaTime;
            float progress = timer / mineDuration;

            if (MiningGaugeUI.instance != null)
            {
                MiningGaugeUI.instance.SetProgress(progress);
            }

            yield return null;
        }

        CompleteMining(pickupHandler);
    }

    private void CompleteMining(ItemPickupHandler pickupHandler)
    {
        if (isMined)
        {
            return;
        }

        isMined = true;
        isMining = false;
        miningCoroutine = null;

        if (MiningGaugeUI.instance != null)
        {
            MiningGaugeUI.instance.Hide();
        }

        pickupHandler.PickupItem(oreItem, amount);

        if (QuestManager.instance != null)
        {
            QuestManager.instance.AddMineCount();
        }

        DespawnOre();
    }

    private void DespawnOre()
    {
        PoolingOre pooledOre = GetComponent<PoolingOre>();

        if (pooledOre != null)
        {
            pooledOre.Despawn();
            return;
        }

        gameObject.SetActive(false);
    }
}