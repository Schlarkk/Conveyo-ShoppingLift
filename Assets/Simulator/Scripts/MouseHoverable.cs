using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseHoverable : MonoBehaviour
{
    public Outline Out;
    public bool forceHover = false;     // force hover on/off externally
    public bool disableHover = false;   // disables outline and hover logic when true

    public DraggableObject DO;

    [Header("UI Sound Settings")]
    public AudioSource audioSource;
    public AudioClip[] uiSounds;

    private int draggableLayerMask;
    private bool isHovered = false;     // NEW: Track hover state

    void Awake()
    {
        Out = GetComponent<Outline>();
        DO = GetComponent<DraggableObject>();
        Out.enabled = false;
        disableHover = false;
    }

    void Start()
    {
        draggableLayerMask = LayerMask.GetMask("DraggableCollider");

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (disableHover)
        {
            Out.enabled = false;
            isHovered = false; // reset state
            return;
        }

        if (forceHover)
        {
            Out.enabled = true;
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, draggableLayerMask))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (!isHovered)
                {
                    OnHoverEnter(); // only call when just entered
                }
                isHovered = true;

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    MoneyManager.GlobalMoney += DO.cost;
                    DO.OBJdestroyer();
                }

                return;
            }
        }

        if (isHovered)
        {
            OnHoverExit(); // only call when just exited
            isHovered = false;
        }
    }

    void OnHoverEnter()
    {
        Out.enabled = true;
        PlayRandomUISound();
    }

    void OnHoverExit()
    {
        Out.enabled = false;
    }

    void PlayRandomUISound()
    {
        if (uiSounds.Length == 0 || audioSource == null) return;

        int index = Random.Range(0, uiSounds.Length);
        audioSource.PlayOneShot(uiSounds[index]);
    }
}



