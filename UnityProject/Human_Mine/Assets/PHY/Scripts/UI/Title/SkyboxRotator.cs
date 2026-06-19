using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [Header("Skybox Rotation")]
    [SerializeField] private float rotationSpeed = 0.5f;

    private Material runtimeSkybox;
    private float currentRotation;

    private void Start()
    {
        if (RenderSettings.skybox == null)
        {
            Debug.LogWarning("Skybox material is missing.");
            enabled = false;
            return;
        }

        runtimeSkybox = Instantiate(RenderSettings.skybox);
        RenderSettings.skybox = runtimeSkybox;

        if (!runtimeSkybox.HasProperty("_Rotation"))
        {
            Debug.LogWarning("This skybox shader does not support _Rotation.");
            enabled = false;
            return;
        }

        currentRotation = runtimeSkybox.GetFloat("_Rotation");
    }

    private void Update()
    {
        currentRotation += rotationSpeed * Time.deltaTime;

        if (currentRotation >= 360f)
        {
            currentRotation -= 360f;
        }

        runtimeSkybox.SetFloat("_Rotation", currentRotation);
    }
}