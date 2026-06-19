using UnityEngine;

public class BlinkText : MonoBehaviour
{
    [Header("Blink")]
    [SerializeField] private float minAlpha = 0.35f;
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private float blinkSpeed = 1.2f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        float t = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
        canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
    }
}
