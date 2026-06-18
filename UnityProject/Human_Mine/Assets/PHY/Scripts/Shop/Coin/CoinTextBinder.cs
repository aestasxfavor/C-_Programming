using System.Collections;
using TMPro;
using UnityEngine;

public class CoinTextBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    private void Awake()
    {
        if (coinText == null)
        {
            coinText = GetComponent<TMP_Text>();
        }

        if (coinText == null)
        {
            coinText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(RegisterRoutine());
    }

    private IEnumerator RegisterRoutine()
    {
        yield return null;

        while (CoinManager.instance == null)
        {
            yield return null;
        }

        if (coinText == null)
        {
            Debug.LogError($"CoinTextBinder 실패: TMP_Text를 못 찾음 / 오브젝트: {gameObject.name}");
            yield break;
        }

        CoinManager.instance.SetCoinText(coinText);
    }
}