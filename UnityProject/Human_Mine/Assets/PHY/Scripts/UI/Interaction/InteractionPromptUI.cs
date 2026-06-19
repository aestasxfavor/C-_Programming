using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;

    private GameObject currentOwner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        HidePrompt();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowPrompt(string message, GameObject owner)
    {
        if (promptPanel == null || promptText == null)
        {
            return;
        }

        currentOwner = owner;
        promptText.text = message;
        promptPanel.SetActive(true);
    }

    public void HidePrompt(GameObject owner)
    {
        if (currentOwner != owner)
        {
            return;
        }

        HidePrompt();
    }

    public void HidePrompt()
    {
        currentOwner = null;

        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
}