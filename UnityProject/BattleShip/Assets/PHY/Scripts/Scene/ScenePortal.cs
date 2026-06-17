using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private LayerMask playerLayer;

    private bool isLoading;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading)
        {
            return;
        }

        if (((1 << other.gameObject.layer) & playerLayer) == 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("ScenePortal: 이동할 씬 이름이 비어 있어요.");
            return;
        }

        isLoading = true;
        SceneManager.LoadScene(targetSceneName);
    }
}