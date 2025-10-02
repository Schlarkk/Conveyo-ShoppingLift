using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TABMeanu : MonoBehaviour
{
    public Animator anim;

    bool tab = true;

    [Header("UI Sound Settings")]
    public AudioSource audioSource;
    public AudioClip[] uiSounds; // Assign 4 sound clips in Inspector

    void Start()
    {
        anim = GetComponent<Animator>();

        // Optional safety check
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (tab)
            {
                anim.SetBool("tab", true);
                tab = false;
                PlayRandomUISound();
            }
            else if (!tab)
            {
                tab = true;
                anim.SetBool("tab", false);
                PlayRandomUISound();
            }
        }

    }
    
    void PlayRandomUISound()
    {
        if (uiSounds.Length == 0 || audioSource == null) return;

        int index = Random.Range(0, uiSounds.Length);
        audioSource.PlayOneShot(uiSounds[index]);
    }
}
