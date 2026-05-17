using UnityEngine;

public class UnlockItem : MonoBehaviour
{
    private bool isCollected;

    private void OnEnable()
    {
        isCollected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected)
        {
            return;
        }

        PlayerAttackController attackController = other.GetComponentInParent<PlayerAttackController>();

        if (attackController == null)
        {
            return;
        }

        isCollected = true;
        attackController.UnlockAttack();

        gameObject.SetActive(false);
    }
}