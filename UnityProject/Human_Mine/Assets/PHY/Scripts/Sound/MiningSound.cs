using System.Collections;
using UnityEngine;

public class MiningSound : MonoBehaviour
{
    [Header("Mining Sound")]
    [SerializeField] private AudioClip miningClip;
    [SerializeField] private int playCount = 3;
    [SerializeField] private float interval = 0.45f;
    [SerializeField] private float volumeScale = 1f;

    private Coroutine miningSoundRoutine;

    public void StartMiningSound()
    {
        StopMiningSound();

        miningSoundRoutine = StartCoroutine(PlayMiningSoundRoutine());
    }

    public void StopMiningSound()
    {
        if (miningSoundRoutine != null)
        {
            StopCoroutine(miningSoundRoutine);
            miningSoundRoutine = null;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopSfx();
        }
    }

    public void FinishMiningSound()
    {
        miningSoundRoutine = null;
    }

    private IEnumerator PlayMiningSoundRoutine()
    {
        for (int i = 0; i < playCount; i++)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySfx(miningClip);
            }

            Debug.Log($"채집 사운드 재생: {i + 1}");

            yield return new WaitForSeconds(interval);
        }

        miningSoundRoutine = null;
    }
}
