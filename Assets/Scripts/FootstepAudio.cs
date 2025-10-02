using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    public AudioClip[] footstepClips;
    public CharacterController controller;

    [Header("Step Timings")]
    public float walkStepInterval = 0.5f;
    public float crouchStepInterval = 0.8f;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float walkVolume = 1f;
    [Range(0f, 1f)] public float crouchVolume = 0.5f;

    private float stepTimer = 0f;
    public AudioSource audioSource;
    private FirstPersonController playerController;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<FirstPersonController>();
    }

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        bool isWalking = controller.isGrounded && (inputX != 0 || inputZ != 0);

        if (isWalking)
        {
            stepTimer -= Time.deltaTime;

            bool isCrouching = playerController != null && playerController.IsCrouching();
            float currentInterval = isCrouching ? crouchStepInterval : walkStepInterval;

            if (stepTimer <= 0f)
            {
                PlayFootstep(isCrouching);
                stepTimer = currentInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayFootstep(bool isCrouching)
    {
        if (footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        float volume = isCrouching ? crouchVolume : walkVolume;

        audioSource.PlayOneShot(footstepClips[index], volume);
    }
}

