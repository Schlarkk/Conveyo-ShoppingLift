using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIHoverableTeleporters : MonoBehaviour
{
    public Outline Out;
    public bool forceHover = false;  // force hover on/off externally
    public bool disableHover = false;

    public GameObject prefabToSpawn;
    public float spawnDistance = 10f;

    // Grid settings
    public float gridOffset = 0.5f;
    public float gridStep = 1.0f;

    private int draggableLayerMask;

    public AudioSource place;

    void Awake()
    {
        Out = GetComponent<Outline>();
        Out.enabled = false;
        disableHover = false;
    }

    void Start()
    {
        draggableLayerMask = LayerMask.GetMask("DraggableCollider");
    }

    void Update()
    {
        if (disableHover)
        {
            Out.enabled = false;
            disableHover = false;
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
                OnHoverEnter();
                return;
            }
        }

        OnHoverExit();
    }

    void OnHoverEnter()
    {
        Out.enabled = true;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            SpawnPrefabForward();
            place.Play();
        }
    }

    void OnHoverExit()
    {
        Out.enabled = false;
    }

    void SpawnPrefabForward()
    {
        Camera cam = Camera.main;
        if (cam == null || prefabToSpawn == null) return;

        Vector3 targetPoint = cam.transform.position + cam.transform.forward * spawnDistance;
        Vector3 snapped = SnapToGrid(targetPoint, gridOffset, gridStep);
        Instantiate(prefabToSpawn, snapped, Quaternion.identity);
    }

    Vector3 SnapToGrid(Vector3 pos, float offset, float step)
    {
        float x = Mathf.Round((pos.x - offset) / step) * step + offset;
        float z = Mathf.Round((pos.z - offset) / step) * step + offset;
        return new Vector3(x, 0f, z); // Y is always 0
    }
}
