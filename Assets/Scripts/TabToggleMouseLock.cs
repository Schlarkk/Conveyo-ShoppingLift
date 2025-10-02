using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabToggleMouseLock : MonoBehaviour
{
    private bool isOpen = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Start with mouse locked
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isOpen = !isOpen;

            if (isOpen)
            {
                // Unlock the mouse
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Lock the mouse
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            // Set animator bool
            if (animator != null)
            {
                animator.SetBool("Open", isOpen);
            }
        }
    }
}
