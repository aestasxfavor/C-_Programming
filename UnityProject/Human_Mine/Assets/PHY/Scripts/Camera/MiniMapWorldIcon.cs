using UnityEngine;
using UnityEngine.UI;

public class MiniMapWorldIcon : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Camera miniMapCamera;
    [SerializeField] private RectTransform miniMapRect;
    [SerializeField] private bool clampToCircle = true;
    [SerializeField] private bool hideWhenOutOfRange = false;

    private RectTransform iconRect;
    private Image iconImage;

    private void Awake()
    {
        iconRect = GetComponent<RectTransform>();
        iconImage = GetComponent<Image>();
    }

    private void LateUpdate()
    {
        if (target == null || miniMapCamera == null || miniMapRect == null)
        {
            return;
        }

        Vector3 viewportPos = miniMapCamera.WorldToViewportPoint(target.position);

        Vector2 iconPos = new Vector2(
            (viewportPos.x - 0.5f) * miniMapRect.rect.width,
            (viewportPos.y - 0.5f) * miniMapRect.rect.height
        );

        float radius = miniMapRect.rect.width * 0.5f - 8f;

        if (clampToCircle && iconPos.magnitude > radius)
        {
            if (hideWhenOutOfRange)
            {
                if (iconImage != null)
                {
                    iconImage.enabled = false;
                }

                return;
            }

            iconPos = iconPos.normalized * radius;
        }

        if (iconImage != null)
        {
            iconImage.enabled = true;
        }

        iconRect.anchoredPosition = iconPos;
    }
}