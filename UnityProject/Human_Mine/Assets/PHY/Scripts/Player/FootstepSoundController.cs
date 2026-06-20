using UnityEngine;

public class FootstepSoundController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private AudioSource audioSource;

    [Header("Footstep Clips")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Settings")]
    [SerializeField] private float moveThreshold = 0.15f;
    [SerializeField] private float stepInterval = 0.45f;
    [SerializeField] private float volume = 0.4f;
    [SerializeField] private float minPitch = 0.92f;
    [SerializeField] private float maxPitch = 1.06f;

    private float stepTimer;

    private void Reset()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (characterController == null || audioSource == null)
        {
            return;
        }

        if (footstepClips == null || footstepClips.Length == 0)
        {
            return;
        }

        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = 0f;

        bool isMoving = horizontalVelocity.magnitude > moveThreshold;
        bool isGrounded = characterController.isGrounded;

        if (!isMoving || !isGrounded)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = stepInterval;
        }
    }

    private void PlayFootstep()
    {
        int index = Random.Range(0, footstepClips.Length);

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(footstepClips[index], volume);
    }
}