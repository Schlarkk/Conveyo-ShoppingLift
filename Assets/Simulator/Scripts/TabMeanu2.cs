using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabMeanu2 : MonoBehaviour
{
    [Header("UI Sound Settings")]
    public AudioSource audioSource;
    public AudioClip[] uiSounds; // Assign 4 sound clips in Inspector


    void Start()
    {
        // Optional safety check
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayRandomUISound();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayRandomUISound();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayRandomUISound();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayRandomUISound();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PlayRandomUISound();
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            PlayRandomUISound();
        }
    }

    void PlayRandomUISound()
    {
        if (uiSounds.Length == 0 || audioSource == null) return;

        int index = Random.Range(0, uiSounds.Length);
        audioSource.PlayOneShot(uiSounds[index]);
    }
}

