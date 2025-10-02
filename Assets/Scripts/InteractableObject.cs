using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    public GameObject primaryTargetObject;
    public GameObject alternateTargetObject;
    public GameObject alternatePermanentObject;
    public GameObject alternatePromptObject;
    public GameObject alternatePromptElement;

    [Tooltip("If true, uses alternate object instead of primary.")]
    public bool useAlternateObject = false;

    [Tooltip("UI prompt to show when looking at this object.")]
    public GameObject interactPromptUI;

    private Transform playerCamera;
    private Coroutine enableCoroutine;
    private bool isLookingAt;

    void Start()
    {
        playerCamera = Camera.main.transform;

        if (primaryTargetObject != null)
            primaryTargetObject.SetActive(false);
        if (alternateTargetObject != null)
            alternateTargetObject.SetActive(false);
        if (alternatePermanentObject != null)
            alternatePermanentObject.SetActive(false);
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);
    }

    void Update()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        bool hitSelf = Physics.Raycast(ray, out hit, interactDistance) && hit.transform == transform;

        if (hitSelf)
        {
            if (!isLookingAt)
            {
                ShowPrompt(true);
                isLookingAt = true;
            }

            if (Input.GetKeyDown(interactKey))
            {
                Interact();
            }
        }
        else if (isLookingAt)
        {
            ShowPrompt(false);
            isLookingAt = false;
        }
    }

    void Interact()
    {
        if (enableCoroutine != null)
            StopCoroutine(enableCoroutine);

        if (useAlternateObject)
        {
            if (alternateTargetObject != null)
            {
                if (alternateTargetObject.activeSelf)
                    alternateTargetObject.SetActive(false);

                enableCoroutine = StartCoroutine(EnableTemporarily(alternateTargetObject, 1.3f));
            }

            if (alternatePermanentObject != null)
            {
                alternatePermanentObject.SetActive(true);
                alternatePromptObject.SetActive(true);
                alternatePromptElement.SetActive(true);
                ShowPrompt(false);
                Destroy(gameObject);
            }
        }
        else
        {
            if (primaryTargetObject != null)
            {
                if (primaryTargetObject.activeSelf)
                    primaryTargetObject.SetActive(false);

                enableCoroutine = StartCoroutine(EnableTemporarily(primaryTargetObject, 1.3f));
            }
        }
    }

    private IEnumerator EnableTemporarily(GameObject targetObject, float duration)
    {
        targetObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        targetObject.SetActive(false);
        enableCoroutine = null;
    }

    private void ShowPrompt(bool show)
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(show);
    }
}
