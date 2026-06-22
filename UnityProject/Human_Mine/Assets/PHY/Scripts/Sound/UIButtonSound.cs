using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private AudioClip clickClip;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"UI 버튼 사운드 입력 감지: {gameObject.name}");

        if (button == null)
        {
            return;
        }

        if (!button.interactable)
        {
            return;
        }

        if (clickClip == null)
        {
            return;
        }

        if (SoundManager.Instance == null)
        {
            return;
        }

        SoundManager.Instance.PlaySfx(clickClip);
    }
}