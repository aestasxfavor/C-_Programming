using System;
using System.Collections;
using UnityEngine;

public class Mole : MonoBehaviour
{
    [Header("Move Setting")]
    [SerializeField] private float riseHeight = 1.2f;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float stayTime = 1f;

    private Vector3 hiddenPos;
    private Vector3 shownPos;

    private bool isClickable = false;
    private bool isHit = false;
    private Coroutine currentRoutine;

    private int currentHoleIndex = -1;
    private Action<int> releaseHoleAction;

    public bool IsActive => gameObject.activeSelf;

    public void SetupPosition(Vector3 spawnPos)
    {
        hiddenPos = spawnPos;
        shownPos = hiddenPos + Vector3.up * riseHeight;
        transform.position = hiddenPos;
    }

    public void ActivateMole(Vector3 spawnPos, int holeIndex, Action<int> releaseAction)
    {
        SetupPosition(spawnPos);

        currentHoleIndex = holeIndex;
        releaseHoleAction = releaseAction;

        gameObject.SetActive(true);

        isClickable = false;
        isHit = false;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        yield return StartCoroutine(MoveTo(shownPos));

        isClickable = true;
        yield return new WaitForSeconds(stayTime);

        isClickable = false;
        yield return StartCoroutine(MoveTo(hiddenPos));

        DeactivateMole();
    }

    private IEnumerator MoveTo(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPos;
    }

    private void OnMouseDown()
    {
        if (!isClickable) return;
        if (isHit) return;

        isHit = true;
        isClickable = false;

        GameManager.Instance.AddScore(10);

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        yield return StartCoroutine(MoveTo(hiddenPos));
        DeactivateMole();
    }

    private void DeactivateMole()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        isClickable = false;
        isHit = false;

        releaseHoleAction?.Invoke(currentHoleIndex);

        currentHoleIndex = -1;
        releaseHoleAction = null;

        gameObject.SetActive(false);
    }

    public void ForceHide()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        isClickable = false;
        isHit = false;

        releaseHoleAction?.Invoke(currentHoleIndex);

        currentHoleIndex = -1;
        releaseHoleAction = null;

        gameObject.SetActive(false);
    }
}