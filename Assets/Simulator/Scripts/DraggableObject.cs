using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class DraggableObject : MonoBehaviour
{
    public Animator anim;
    public GameObject animOBJ;
    public float liftHeight = 1.2f;

    public float cost;

    public AudioClip Grab;
    public AudioClip PlaceDown;
    public AudioClip Turn;
    private AudioSource ASs;

    public bool Dropper;
    public bool Energy;
    public bool Material;
    public PrefabSpawner ps;

    public bool tpentrance;
    public int GlobalTpet;

    public bool tpexit;
    public int GlobalTpex;

    public GameObject duplicatePrefab;
    public float duplicateYOffset = 1.2f;

    public GameObject RemoveParticles;
    public GameObject Arrows;

    public GameObject ghostPrefab;
    private GameObject currentGhost;

    // ======== HANDS (drag uses Pointing/Grabbing only; Tab manager uses Holding) ========
    public GameObject GrabbingGO;   // shown when NOT dragging (and NOT in Tab build mode)
    public GameObject PointingGO;   // shown while dragging (only if NOT in Tab build mode)
    public GameObject HoldingGO;    // optional ref to clamp OFF during non-build-mode drags

    // ======== DRAG HEIGHT CONTROL (Shift + Mouse Wheel) ========
    [Header("Drag Height Control")]
    [Tooltip("Meters per Shift+Scroll step while dragging.")]
    public float heightStep = 0.5f;
    public float minHeight = 0f;
    public float maxHeight = 200f;

    // ======== GRID / OCCUPANCY ========
    [Header("Grid / Occupancy")]
    [Tooltip("Radius (XZ) used to probe whether a grid cell is occupied.")]
    public float occupancyRadius = 0.45f;
    [Tooltip("How far (in cells) to search outward when the intended cell is occupied.")]
    public int maxSpiralRadius = 32;

    // public so palette can count active drags
    public static int ActiveDragCount = 0;

    private bool isDragging = false;
    private Plane dragPlane;
    private Vector3 offset;
    private float originalY;

    private MouseHoverable hoverable;
    private int draggableLayerMask;

    void Start()
    {
        if (Dropper) ps = GetComponent<PrefabSpawner>();
        if (anim == null && animOBJ != null)
            anim = animOBJ.GetComponent<Animator>();

        hoverable = GetComponent<MouseHoverable>();
        ASs = GetComponent<AudioSource>();
        originalY = transform.position.y;

        // Must match your DraggableCollider layer
        draggableLayerMask = LayerMask.GetMask("DraggableCollider");

        if (Arrows != null) Arrows.SetActive(false);

        // Find hands if not assigned (includes inactive objects)
        if (GrabbingGO == null) GrabbingGO = FindSceneObjectByName("Grabbing");
        if (PointingGO == null) PointingGO = FindSceneObjectByName("Pointing");
        if (HoldingGO  == null) HoldingGO  = FindSceneObjectByName("Holding");

        // Default: show grabbing if not in build mode
        if (!TabpressSomething.BuildModeActive)
        {
            if (GrabbingGO) GrabbingGO.SetActive(true);
            if (PointingGO) PointingGO.SetActive(false);
            if (HoldingGO)  HoldingGO.SetActive(false);
        }

        if (Dropper && Energy && Material && ps != null)
            ps.enabled = true;
    }

    void Awake()
    {
        if (Arrows != null) Arrows.SetActive(false);
        if (Dropper && Energy && Material && ps != null)
            ps.enabled = true;
    }

    void Update()
    {
        // Keep spawner enable state in sync
        if (Dropper && Energy && !isDragging && Material)
        {
            if (ps != null) ps.enabled = true;
        }
        else if ((Dropper && !Energy && !isDragging) || (!Material && Dropper && !isDragging))
        {
            if (ps != null) ps.enabled = false;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // -------- Grab start --------
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, draggableLayerMask))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    isDragging = true;
                    ActiveDragCount++;
                    anim?.SetBool("drag", true);
                    if (Arrows != null) Arrows.SetActive(true);
                    if (ASs != null && Grab != null) { ASs.clip = Grab; ASs.Play(); }

                    if (Dropper && ps != null)
                        ps.enabled = false;

                    dragPlane = new Plane(Vector3.up, transform.position);

                    if (dragPlane.Raycast(ray, out float enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        offset = transform.position - hitPoint;
                        originalY = Mathf.Clamp(transform.position.y, minHeight, maxHeight);

                        // Lift object on grab
                        transform.position = new Vector3(transform.position.x, originalY + liftHeight, transform.position.z);

                        if (hoverable != null)
                            hoverable.forceHover = true;
                    }

                    // ---- HANDS: only switch to Pointing if NOT in Tab build mode ----
                    if (!TabpressSomething.BuildModeActive)
                    {
                        if (HoldingGO && HoldingGO.activeSelf) HoldingGO.SetActive(false); // clamp OFF
                        if (GrabbingGO) GrabbingGO.SetActive(false);
                        if (PointingGO) PointingGO.SetActive(true);
                    }

                    if (ghostPrefab != null && currentGhost == null)
                    {
                        currentGhost = Instantiate(ghostPrefab);
                        currentGhost.transform.rotation = transform.rotation;
                    }
                }
            }
        }

        // -------- Grab end --------
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            ActiveDragCount = Mathf.Max(0, ActiveDragCount - 1);
            anim?.SetBool("drag", false);
            if (Arrows != null) Arrows.SetActive(false);
            if (ASs != null && PlaceDown != null) { ASs.clip = PlaceDown; ASs.Play(); }

            if (Dropper && Energy && Material && ps != null)
                ps.enabled = true;

            // Figure intended grid cell from current XZ (before moving)
            Vector2Int intendedCell = new Vector2Int(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.z)
            );

            // Find nearest free cell and snap there (prevents stacking)
            if (!FindNearestFreeCellPhysics(intendedCell, originalY, maxSpiralRadius, out Vector2Int chosenCell))
            {
                chosenCell = intendedCell; // fallback
            }

            // Snap to grid at the chosen free cell and keep the adjusted Y (originalY)
            float snappedX = chosenCell.x + 0.5f;
            float snappedZ = chosenCell.y + 0.5f;
            transform.position = new Vector3(snappedX, originalY, snappedZ);

            if (hoverable != null)
                hoverable.forceHover = false;

            if (currentGhost != null)
            {
                Destroy(currentGhost);
                currentGhost = null;
            }

            // ---- HANDS: revert only if NOT in Tab build mode ----
            if (!TabpressSomething.BuildModeActive)
            {
                if (PointingGO) PointingGO.SetActive(false);
                if (GrabbingGO) GrabbingGO.SetActive(true);
                if (HoldingGO)  HoldingGO.SetActive(false); // clamp OFF
            }
        }

        // -------- While dragging --------
        if (isDragging)
        {
            // --- Shift + Mouse Wheel height control (replaces arrow keys) ---
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float scroll = Input.mouseScrollDelta.y;
            if (shift && Mathf.Abs(scroll) > 0.01f)
            {
                originalY = Mathf.Clamp(originalY + Mathf.Sign(scroll) * heightStep, minHeight, maxHeight);
                dragPlane = new Plane(Vector3.up, new Vector3(0, originalY, 0));
            }

            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 targetPos = hitPoint + offset;

                // ---- Clamp to 20m from camera to prevent "drag to infinity" ----
                targetPos = ClampToCameraRange(targetPos, 20f);

                transform.position = new Vector3(targetPos.x, originalY + liftHeight, targetPos.z);

                if (currentGhost != null)
                {
                    float ghostX = Mathf.Floor(targetPos.x) + 0.5f;
                    float ghostZ = Mathf.Floor(targetPos.z) + 0.5f;
                    Vector3 ghostPos = new Vector3(ghostX, originalY, ghostZ);
                    currentGhost.transform.position = ghostPos;
                    currentGhost.transform.rotation = transform.rotation;
                }
            }

            // rotation
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (ASs != null && Turn != null) { ASs.clip = Turn; ASs.Play(); }
                transform.Rotate(0, 90, 0);
            }

            // duplication
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (MoneyManager.GlobalMoney >= cost)
                {
                    if (ASs != null && PlaceDown != null) { ASs.clip = PlaceDown; ASs.Play(); }
                    MoneyManager.GlobalMoney -= cost;
                    DuplicateBelow();
                }
            }

            // remover
            if (Input.GetKeyDown(KeyCode.Q))
            {
                MoneyManager.GlobalMoney += cost;
                OBJdestroyer();
                return;
            }
        }

        // Extra clamp: if NOT in build mode, Holding must stay OFF even if something else toggles it.
        if (!TabpressSomething.BuildModeActive && HoldingGO && HoldingGO.activeSelf)
            HoldingGO.SetActive(false);
    }

    private Vector3 ClampToCameraRange(Vector3 desired, float maxDistance)
    {
        if (Camera.main == null || maxDistance <= 0f) return desired;
        Vector3 camPos = Camera.main.transform.position;
        Vector3 delta = desired - camPos;
        float dist = delta.magnitude;
        if (dist > maxDistance) return camPos + delta.normalized * maxDistance;
        return desired;
    }

    public void OBJdestroyer()
    {
        if (RemoveParticles != null)
            Instantiate(RemoveParticles, transform.position, transform.rotation);
        Destroy(gameObject);
        if (currentGhost != null)
            Destroy(currentGhost);
    }

    private bool IsHalfStepped(float value)
    {
        float remainder = Mathf.Abs(value % 1f);
        return Mathf.Abs(remainder - 0.5f) < 0.01f;
    }

    private void DuplicateBelow()
    {
        if (duplicatePrefab == null)
        {
            Debug.LogWarning("No duplicatePrefab assigned!");
            return;
        }

        float snappedX = Mathf.Floor(transform.position.x) + 0.5f;
        float snappedZ = Mathf.Floor(transform.position.z) + 0.5f;
        Vector3 spawnPos = new Vector3(snappedX, transform.position.y - duplicateYOffset, snappedZ);

        if (!IsHalfStepped(spawnPos.x) || !IsHalfStepped(spawnPos.z))
        {
            Debug.Log("X or Z not aligned to .5-step. Cannot place.");
            return;
        }

        BoxCollider prefabCollider = duplicatePrefab.GetComponent<BoxCollider>();
        if (prefabCollider == null)
        {
            Debug.LogWarning("Duplicate prefab needs a BoxCollider for placement checking!");
            return;
        }

        GameObject clone = Instantiate(duplicatePrefab, spawnPos, transform.rotation);

        // --- Save hook for duplicates too ---
        var po = clone.GetComponent<PlacedObject>();
        if (po == null) po = clone.AddComponent<PlacedObject>();
        po.SetPrefabKey(duplicatePrefab.name); // use duplicate prefab name as key

        if (SaveManager.Instance != null && SaveManager.Instance.DynamicRoot != null)
        clone.transform.SetParent(SaveManager.Instance.DynamicRoot, true);


        // Assign unique IDs separately for entrances and exits
        TeleportEntrance[] entrances = clone.GetComponentsInChildren<TeleportEntrance>();
        foreach (var entrance in entrances)
            entrance.targetTeleportID = TeleportIDManager.GetUniqueEntranceID();

        TeleportExit[] exits = clone.GetComponentsInChildren<TeleportExit>();
        foreach (var exit in exits)
            exit.teleportID = TeleportIDManager.GetUniqueExitID();

        MouseHoverable cloneHover = clone.GetComponent<MouseHoverable>();
        if (cloneHover != null)
        {
            cloneHover.forceHover = false;

            if (cloneHover.Out != null)
            {
                cloneHover.Out.enabled = false;

                var renderers = clone.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    var mats = new List<Material>(renderer.sharedMaterials);
                    mats.RemoveAll(m => m != null && (m.name.Contains("OutlineMask") || m.name.Contains("OutlineFill")));
                    renderer.materials = mats.ToArray();
                }
            }
        }
    }

    private GameObject FindSceneObjectByName(string name)
    {
        var all = Resources.FindObjectsOfTypeAll<GameObject>(); // includes inactive + DontDestroyOnLoad
        foreach (var go in all)
        {
            if (go == null) continue;
            if (go.name != name) continue;
            if (!go.scene.IsValid()) continue;               // skip project assets/prefabs
            if (go.hideFlags != HideFlags.None) continue;    // skip hidden editor objs
            return go;
        }
        return null;
    }

    // ======== OCCUPANCY / SPIRAL SEARCH ========

    private bool FindNearestFreeCellPhysics(Vector2Int start, float y, int maxRadius, out Vector2Int result)
    {
        // If start is free, use it.
        if (!IsCellOccupiedPhysics(start, y)) { result = start; return true; }

        // Spiral outwards in Manhattan rings: r = 1..maxRadius
        for (int r = 1; r <= maxRadius; r++)
        {
            // Top & bottom rows
            for (int dx = -r; dx <= r; dx++)
            {
                var c1 = new Vector2Int(start.x + dx, start.y + r);
                if (!IsCellOccupiedPhysics(c1, y)) { result = c1; return true; }

                var c2 = new Vector2Int(start.x + dx, start.y - r);
                if (!IsCellOccupiedPhysics(c2, y)) { result = c2; return true; }
            }
            // Left & right columns (exclude corners already checked)
            for (int dz = -r + 1; dz <= r - 1; dz++)
            {
                var c3 = new Vector2Int(start.x - r, start.y + dz);
                if (!IsCellOccupiedPhysics(c3, y)) { result = c3; return true; }

                var c4 = new Vector2Int(start.x + r, start.y + dz);
                if (!IsCellOccupiedPhysics(c4, y)) { result = c4; return true; }
            }
        }

        result = start;
        return false;
    }

    private bool IsCellOccupiedPhysics(Vector2Int cell, float y)
    {
        // Big vertical half-extent so height doesn’t matter
        Vector3 center = new Vector3(cell.x + 0.5f, y, cell.y + 0.5f);
        Vector3 halfExtents = new Vector3(occupancyRadius, 1000f, occupancyRadius);

        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i]; if (!col) continue;

            // ignore this object's drag ghost if any
            if (currentGhost != null && col.transform.IsChildOf(currentGhost.transform)) continue;

            // only consider other draggables
            var drag = col.GetComponentInParent<DraggableObject>();
            if (!drag) continue;
            if (drag == this) continue; // <- ignore self

            Vector3 p = drag.transform.position;
            Vector2Int other = new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.z));
            if (other == cell) return true;
        }
        return false;
    }
}
