using UnityEngine;

public class UnLockItem : MonoBehaviour
{
    private bool hasCollected;

    private void OnEnable()
    {
        hasCollected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollected)
        {
            return;
        }

        PlayerAttackController attackController = other.GetComponentInParent<PlayerAttackController>();

        if (attackController == null)
        {
            return;
        }

        hasCollected = true;

        attackController.UnlockAttack();

        Debug.Log("Attack Item Collected. Auto attack unlocked.");

        gameObject.SetActive(false);
    }

}
