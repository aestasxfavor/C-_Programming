using UnityEngine;

public class MiningGaugeUI : MonoBehaviour
{
    public static MiningGaugeUI instance;

    [Header("Root")]
    [SerializeField] private GameObject gaugeRoot;

    [Header("Asset Gauge")]
    [SerializeField] private ImgsFillDynamic fillDynamic;

    private void Awake()
    {
        instance = this;
        Hide();
    }

    public void Show()
    {
        if (gaugeRoot != null)
        {
            gaugeRoot.SetActive(true);
        }

        SetProgress(0f);
    }

    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        if (fillDynamic != null)
        {
            fillDynamic.SetValue(progress, true);
        }
    }

    public void Hide()
    {
        if (fillDynamic != null)
        {
            fillDynamic.SetValue(0f, true);
        }

        if (gaugeRoot != null)
        {
            gaugeRoot.SetActive(false);
        }
    }
}