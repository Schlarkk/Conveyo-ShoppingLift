using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabpressSomething: MonoBehaviour
{
    // ======== GLOBAL FLAG: true while Tab build palette is active ========
    public static bool BuildModeActive { get; private set; } = false;

    public enum Category { All = 0, Conveyers = 1, Furnaces = 2, Droppers = 3, Extras = 4, Walls = 5 }

    [Header("Conveyers")]
    public List<GameObject> Conveyers = new List<GameObject>();
    public List<GameObject> ConveyerGhosts = new List<GameObject>();
    public List<float> ConveyerPrices = new List<float>();

    [Header("Furnaces")]
    public List<GameObject> Furnaces = new List<GameObject>();
    public List<GameObject> FurnaceGhosts = new List<GameObject>();
    public List<float> FurnacePrices = new List<float>();

    [Header("Droppers")]
    public List<GameObject> Droppers = new List<GameObject>();
    public List<GameObject> DropperGhosts = new List<GameObject>();
    public List<float> DropperPrices = new List<float>();

    [Header("Extras")]
    public List<GameObject> Extras = new List<GameObject>();
    public List<GameObject> ExtraGhosts = new List<GameObject>();
    public List<float> ExtraPrices = new List<float>();

    [Header("Walls")]
    public List<GameObject> Walls = new List<GameObject>();
    public List<GameObject> WallGhosts = new List<GameObject>();
    public List<float> WallPrices = new List<float>();

    [Header("UI (optional)")]
    public Text itemNameLabel;
    public Text priceLabel;
    public Image iconImage;

    [Header("Fixed Visual Preview")]
    public Transform visualPreviewAnchor;
    public bool parentPreviewToAnchor = true;
    public bool resetLocalOnPreview = true;

    [Header("Placement / Ghost")]
    public LayerMask placementSurfaceMask;
    public bool requireSurfaceHit = false;
    public float occupancyRadius = 0.45f;
    public int maxSpiralRadius = 32;
    public float ghostMaxDistanceFromCamera = 20f;
    public bool forceSafeOnClones = true;

    [Header("UI Sound Settings")]
    public AudioSource audioSource;
    public AudioClip[] uiSounds; // Assign 4 sound clips in Inspector

    public AudioClip TabEnable;
    public AudioClip[] Scroll;

    [Header("Input")]
    public KeyCode placeKey = KeyCode.Mouse0;
    public KeyCode prevKey  = KeyCode.LeftBracket;   // '['
    public KeyCode nextKey  = KeyCode.RightBracket;  // ']'

    [Header("Rotation (Build Mode)")]
    [Tooltip("Degrees per press; 90 recommended for grid builds.")]
    public float rotationStep = 90f;
    [Tooltip("Rotate clockwise.")]
    public KeyCode rotateCWKey = KeyCode.R;
    [Tooltip("Rotate counter-clockwise (optional). Shift+R also rotates CCW).")]
    public KeyCode rotateCCWKey = KeyCode.T;

    [Header("Height Control (Build Mode)")]
    [Tooltip("Meters per Shift+Scroll step.")]
    public float heightStep = 0.5f;
    public float minHeight = 0f;
    public float maxHeight = 200f;
    [Tooltip("When entering build mode, initialize height from the surface under the cursor if possible.")]
    public bool setInitialHeightFromSurface = true;

    [Header("Layers")]
    public string draggableLayerName = "DraggableCollider"; // used by DraggableObject
    public string lockLayerName      = "Default";           // MUST NOT equal draggableLayerName
    public string uiVisualLayerName  = "UI";                // preview only; ghost keeps its prefab layer

    [Header("Behavior")]
    public bool startLocked = false;

    [Header("Hands (Tab uses Holding; drag uses Pointing)")]
    public GameObject GrabbingGO;   // UNLOCKED
    public GameObject HoldingGO;    // LOCKED (Tab)
    public GameObject PointingGO;   // forced OFF during Tab; drag script controls it while dragging

    // ----- runtime buffers -----
    private struct ItemEntry
    {
        public string displayName;
        public GameObject prefab;
        public GameObject ghostPrefab;
        public float price;
        public Category category;
    }

    private List<ItemEntry> listAll = new List<ItemEntry>();
    private List<ItemEntry> listConvey = new List<ItemEntry>();
    private List<ItemEntry> listFurnace = new List<ItemEntry>();
    private List<ItemEntry> listDropper = new List<ItemEntry>();
    private List<ItemEntry> listExtra = new List<ItemEntry>();
    private List<ItemEntry> listWall = new List<ItemEntry>();
    private List<ItemEntry> _activeList;
    private int _selIndex = 0;

    private bool _locked;
    private readonly Dictionary<GameObject, int> _changedLayers = new Dictionary<GameObject, int>();
    private Camera _cam;
    private int _lockLayer;
    private int _uiVisualLayer;

    // Ghost & Preview & rotation & height
    private GameObject _ghost;
    private Vector2Int _ghostCell;
    private float _ghostY;
    private GameObject _visualPreview;
    private float _currentYaw = 0f;     // rotation state
    private float _placementY = 0f;     // build-mode height

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetBuildFlagOnDomainReload() { BuildModeActive = false; }

    void Start()
    {
        _cam = Camera.main;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        _lockLayer = LayerMask.NameToLayer(lockLayerName);
        if (_lockLayer < 0) { Debug.LogWarning($"[BuildPalette] lockLayer '{lockLayerName}' not found. Using Default(0)."); _lockLayer = 0; }

        _uiVisualLayer = LayerMask.NameToLayer(uiVisualLayerName);
        if (_uiVisualLayer < 0) { Debug.LogWarning($"[BuildPalette] UI layer '{uiVisualLayerName}' not found. Using Default(0)."); _uiVisualLayer = 0; }

        // Sanity: lock layer must not equal draggable layer
        int dragLayer = LayerMask.NameToLayer(draggableLayerName);
        if (dragLayer == _lockLayer)
            Debug.LogError($"[BuildPalette] lockLayerName ('{lockLayerName}') MUST NOT equal draggableLayerName ('{draggableLayerName}'). Change lockLayerName to a layer excluded by DraggableObject raycasts (e.g., 'Default' or 'Ignore Raycast').");

        EnsureHandsRefs();

        BuildRuntimeLists();
        SetCategory(Category.All);

        ApplyLock(startLocked); // also sets initial height if configured
    }

    void OnDestroy()
    {
        DestroyGhostIfAny();
        DestroyPreviewIfAny();
        SetHandsForLock(false);
        BuildModeActive = false;
    }

    void Update()
    {
        // Authoritative hands
        EnforceHoldingVisual();

        // Toggle Tab (block while dragging)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (DraggableObject.ActiveDragCount > 0) return;
            ApplyLock(!_locked);

            audioSource.clip = TabEnable;
            audioSource.Play();
        }

        // Category hotkeys (block while dragging)
        if (DraggableObject.ActiveDragCount == 0)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetCategory(Category.All);
                PlayRandomUISound();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetCategory(Category.Conveyers);
                PlayRandomUISound();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetCategory(Category.Furnaces);
                PlayRandomUISound();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetCategory(Category.Droppers);
                PlayRandomUISound();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SetCategory(Category.Extras);
                PlayRandomUISound();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SetCategory(Category.Walls);
                PlayRandomUISound();
            }    
        }

        if (_locked)
        {
            HandleSelectionInput();  // (ignores Shift+scroll)
            HandleRotationInput();
            HandleHeightScroll();    // <<< Shift+scroll height >>>
            UpdateGhost();
            HandlePlacementInput();
        }
    }

    void PlayRandomUISound()
    {
        if (uiSounds.Length == 0 || audioSource == null) return;

        int index = UnityEngine.Random.Range(0, uiSounds.Length);
        audioSource.PlayOneShot(uiSounds[index]);
    }

    void PlayRandomScrollSound()
    {
        if (Scroll.Length == 0 || audioSource == null) return;

        int index = UnityEngine.Random.Range(0, Scroll.Length);
        audioSource.PlayOneShot(Scroll[index]);
    }

    private void EnforceHoldingVisual()
    {
        if (HoldingGO) HoldingGO.SetActive(BuildModeActive);
        if (PointingGO && BuildModeActive) PointingGO.SetActive(false);
        if (GrabbingGO && BuildModeActive) GrabbingGO.SetActive(false);
        if (GrabbingGO && !BuildModeActive && DraggableObject.ActiveDragCount == 0)
            GrabbingGO.SetActive(true);
    }

    // ---------- selection ----------
    private void BuildRuntimeLists()
    {
        listConvey.Clear(); listFurnace.Clear(); listDropper.Clear(); listExtra.Clear(); listWall.Clear(); listAll.Clear();
        BuildForCategory(Conveyers, ConveyerGhosts, ConveyerPrices, Category.Conveyers, ref listConvey);
        BuildForCategory(Furnaces,  FurnaceGhosts,  FurnacePrices,  Category.Furnaces,  ref listFurnace);
        BuildForCategory(Droppers,  DropperGhosts,  DropperPrices,  Category.Droppers,  ref listDropper);
        BuildForCategory(Extras,    ExtraGhosts,    ExtraPrices,    Category.Extras,    ref listExtra);
        BuildForCategory(Walls,     WallGhosts,     WallPrices,     Category.Walls,     ref listWall);

        listAll.AddRange(listConvey);
        listAll.AddRange(listFurnace);
        listAll.AddRange(listDropper);
        listAll.AddRange(listExtra);
        listAll.AddRange(listWall);
    }

    private void BuildForCategory(List<GameObject> items, List<GameObject> ghosts, List<float> prices, Category cat, ref List<ItemEntry> outList)
    {
        int n = Mathf.Min(items?.Count ?? 0, ghosts?.Count ?? 0);
        if (items == null || ghosts == null) { Debug.LogWarning($"[BuildPalette] '{cat}': Provide both item and ghost lists."); return; }
        if (items.Count != ghosts.Count)
            Debug.LogWarning($"[BuildPalette] '{cat}': Item/Ghost counts differ (items={items.Count}, ghosts={ghosts.Count}). Using min={n}.");

        for (int i = 0; i < n; i++)
        {
            var prefab = items[i];
            var ghost  = ghosts[i];
            if (!prefab || !ghost) continue;

            float price = (prices != null && i < prices.Count) ? prices[i] : 0f;

            outList.Add(new ItemEntry
            {
                displayName = prefab.name,
                prefab = prefab,
                ghostPrefab = ghost,
                price = price,
                category = cat
            });
        }
        if (prices != null && prices.Count < (items?.Count ?? 0))
            Debug.LogWarning($"[BuildPalette] '{cat}': Prices shorter than items; missing prices default to 0.");
    }

    private void SetCategory(Category cat)
    {
        _activeList =
            cat == Category.Conveyers ? listConvey :
            cat == Category.Furnaces  ? listFurnace :
            cat == Category.Droppers  ? listDropper :
            cat == Category.Extras    ? listExtra :
            cat == Category.Walls     ? listWall   :
            listAll;

        _selIndex = (_activeList == null || _activeList.Count == 0) ? 0 : Mathf.Clamp(_selIndex, 0, _activeList.Count - 1);
        UpdateUI();
        if (_locked) { RebuildGhostForCurrent(); RebuildPreviewForCurrent(); ApplyCurrentYaw(); }
    }

    private ItemEntry? Current()
    {
        if (_activeList == null || _activeList.Count == 0) return null;
        if (_selIndex < 0 || _selIndex >= _activeList.Count) return null;
        return _activeList[_selIndex];
    }

    private void HandleSelectionInput()
    {
        // Do NOT consume scroll if Shift is held (reserved for height)
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (shift) return;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0.01f)
        {
            Next();
            PlayRandomScrollSound();
        }
        else if (scroll < -0.01f)
        {
            Prev();
            PlayRandomScrollSound();
        }

        if (Input.GetKeyDown(prevKey))
        {
            Prev();
            PlayRandomScrollSound();
        }
        if (Input.GetKeyDown(nextKey))
        {
            Next();
            PlayRandomScrollSound();
        }
    }

    private void Next() { if (_activeList?.Count > 0) { _selIndex = (_selIndex + 1) % _activeList.Count; UpdateUI(); RebuildGhostForCurrent(); RebuildPreviewForCurrent(); ApplyCurrentYaw(); } }
    private void Prev() { if (_activeList?.Count > 0) { _selIndex = (_selIndex - 1 + _activeList.Count) % _activeList.Count; UpdateUI(); RebuildGhostForCurrent(); RebuildPreviewForCurrent(); ApplyCurrentYaw(); } }

    private void UpdateUI()
    {
        var cur = Current();
        if (cur.HasValue)
        {
            if (itemNameLabel) itemNameLabel.text = cur.Value.displayName;
            if (priceLabel)    priceLabel.text    = $"${cur.Value.price:0}";
        }
        else
        {
            if (itemNameLabel) itemNameLabel.text = "(no items)";
            if (priceLabel)    priceLabel.text    = "";
        }
    }

    // ---------- rotation ----------
    private void HandleRotationInput()
    {
        bool ccwFromShift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(rotateCWKey);
        bool ccwFromKey   = (rotateCCWKey != KeyCode.None) && Input.GetKeyDown(rotateCCWKey);
        bool cwFromKey    = Input.GetKeyDown(rotateCWKey) && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        if (cwFromKey)
        {
            _currentYaw = Mathf.Repeat(_currentYaw + rotationStep, 360f);
            ApplyCurrentYaw();
        }
        else if (ccwFromShift || ccwFromKey)
        {
            _currentYaw = Mathf.Repeat(_currentYaw - rotationStep, 360f);
            ApplyCurrentYaw();
        }
    }

    private void ApplyCurrentYaw()
    {
        if (_ghost) _ghost.transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);

        if (_visualPreview)
        {
            if (parentPreviewToAnchor)
                _visualPreview.transform.localRotation = Quaternion.Euler(0f, _currentYaw, 0f);
            else
            {
                if (visualPreviewAnchor)
                    _visualPreview.transform.rotation = visualPreviewAnchor.rotation * Quaternion.Euler(0f, _currentYaw, 0f);
                else
                    _visualPreview.transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
            }
        }
    }

    // ---------- height (Shift + scroll) ----------
    private void HandleHeightScroll()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (!shift) return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        // Positive scroll -> up; negative -> down
        _placementY = Mathf.Clamp(_placementY + Mathf.Sign(scroll) * heightStep, minHeight, maxHeight);
    }

    // ---------- ghost ----------
    private void EnsureGhostForCurrent()
    {
        var cur = Current();
        if (!cur.HasValue || !cur.Value.ghostPrefab) { DestroyGhostIfAny(); return; }
        if (_ghost && _ghost.name == GhostNameFor(cur.Value.ghostPrefab)) return;

        DestroyGhostIfAny();
        _ghost = Instantiate(cur.Value.ghostPrefab);
        _ghost.name = GhostNameFor(cur.Value.ghostPrefab);
        if (forceSafeOnClones) MakeObjectSafe(_ghost);
        ApplyCurrentYaw();
    }
    private void RebuildGhostForCurrent() { if (_locked) { DestroyGhostIfAny(); EnsureGhostForCurrent(); } }
    private void DestroyGhostIfAny() { if (_ghost) Destroy(_ghost); _ghost = null; }
    private string GhostNameFor(GameObject prefab) => $"__GHOST__{prefab.name}";

    private void UpdateGhost()
    {
        if (!_ghost) { EnsureGhostForCurrent(); if (!_ghost) return; }

        // We still compute aimPos to get X/Z from mouse; Y comes from _placementY
        if (!TryGetAimPoint(out Vector3 aimPos))
        {
            if (requireSurfaceHit) { _ghost.SetActive(false); return; }
        }

        _ghost.SetActive(true);

        // Clamp distance for the *target* point (we will override Y)
        if (ghostMaxDistanceFromCamera > 0f)
            aimPos = ClampToCameraRange(aimPos, ghostMaxDistanceFromCamera);

        Vector2Int intendedCell = new Vector2Int(Mathf.FloorToInt(aimPos.x), Mathf.FloorToInt(aimPos.z));
        float y = _placementY; // <<< use controlled height >>>

        if (!FindNearestFreeCellPhysics(intendedCell, y, maxSpiralRadius, out Vector2Int chosenCell))
        {
            _ghost.transform.position = new Vector3(intendedCell.x + 0.5f, y - 10f, intendedCell.y + 0.5f);
            return;
        }

        _ghostCell = chosenCell;
        _ghostY = y;
        _ghost.transform.position = new Vector3(chosenCell.x + 0.5f, y, chosenCell.y + 0.5f);
        _ghost.transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
    }

    // ---------- preview ----------
    private void EnsurePreviewForCurrent()
    {
        var cur = Current();
        if (!cur.HasValue || !cur.Value.prefab || !visualPreviewAnchor) { DestroyPreviewIfAny(); return; }
        if (_visualPreview && _visualPreview.name == PreviewNameFor(cur.Value.prefab)) return;

        DestroyPreviewIfAny();
        _visualPreview = Instantiate(cur.Value.prefab);
        _visualPreview.name = PreviewNameFor(cur.Value.prefab);
        if (forceSafeOnClones) MakeObjectSafe(_visualPreview);
        SetLayerRecursive(_visualPreview, _uiVisualLayer);

        if (parentPreviewToAnchor)
        {
            _visualPreview.transform.SetParent(visualPreviewAnchor, false);
            if (resetLocalOnPreview)
            {
                _visualPreview.transform.localPosition = Vector3.zero;
                _visualPreview.transform.localScale = Vector3.one;
            }
            _visualPreview.transform.localRotation = Quaternion.Euler(0f, _currentYaw, 0f);
        }
        else
        {
            _visualPreview.transform.position = visualPreviewAnchor.position;
            _visualPreview.transform.rotation = visualPreviewAnchor.rotation * Quaternion.Euler(0f, _currentYaw, 0f);
            _visualPreview.transform.localScale = visualPreviewAnchor.lossyScale;
        }
    }
    private void RebuildPreviewForCurrent() { if (_locked) { DestroyPreviewIfAny(); EnsurePreviewForCurrent(); } }
    private void DestroyPreviewIfAny() { if (_visualPreview) Destroy(_visualPreview); _visualPreview = null; }
    private string PreviewNameFor(GameObject prefab) => $"__PREVIEW__{prefab.name}";

    // ---------- placement ----------
    private void HandlePlacementInput()
    {
        if (!Input.GetKeyDown(placeKey)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        var cur = Current();
        if (!cur.HasValue) { Debug.Log("[BuildPalette] No item to place."); return; }
        var item = cur.Value;

        if (MoneyManager.GlobalMoney < item.price) { Debug.Log("[BuildPalette] Not enough money."); return; }

        // We still need X/Z from aim; Y comes from _placementY
        if (!TryGetAimPoint(out Vector3 aimPos)) { if (requireSurfaceHit) { Debug.Log("[BuildPalette] No valid surface."); return; } }
        if (ghostMaxDistanceFromCamera > 0f) aimPos = ClampToCameraRange(aimPos, ghostMaxDistanceFromCamera);

        Vector2Int intendedCell = new Vector2Int(Mathf.FloorToInt(aimPos.x), Mathf.FloorToInt(aimPos.z));
        float y = _placementY; // <<< use controlled height >>>

        if (!FindNearestFreeCellPhysics(intendedCell, y, maxSpiralRadius, out Vector2Int chosenCell))
        {
            Debug.Log("[BuildPalette] No free grid cell found nearby.");
            return;
        }

        Vector3 spawnPos = new Vector3(chosenCell.x + 0.5f, y, chosenCell.y + 0.5f);
        Quaternion spawnRot = Quaternion.Euler(0f, _currentYaw, 0f);

        MoneyManager.GlobalMoney -= item.price;
        GameObject clone = Instantiate(item.prefab, spawnPos, spawnRot);

        // --- Save hook: tag the placed instance with its prefab key ---
        var po = clone.GetComponent<PlacedObject>();
        if (po == null) po = clone.AddComponent<PlacedObject>();
        po.SetPrefabKey(item.prefab.name); // or your own key if you want an alias

        // (optional) parent under SaveManager’s dynamic root
        if (SaveManager.Instance != null && SaveManager.Instance.DynamicRoot != null)
        clone.transform.SetParent(SaveManager.Instance.DynamicRoot, true);


        if (_locked && clone) RememberAndLockLayerRecursive(clone, _lockLayer);

        Debug.Log($"[BuildPalette] Placed {item.displayName} at {chosenCell} (y={y:0.00}) rot {Mathf.RoundToInt(_currentYaw)}° for ${item.price:0}");
    }

    // ---------- utility ----------
    private bool TryGetAimPoint(out Vector3 aimPos)
    {
        aimPos = Vector3.zero;
        if (!_cam) _cam = Camera.main; if (!_cam) return false;
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        if (placementSurfaceMask.value != 0)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementSurfaceMask, QueryTriggerInteraction.Ignore))
            { aimPos = hit.point; return true; }
            if (requireSurfaceHit) return false;
        }

        var plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float enter)) { aimPos = ray.GetPoint(enter); return true; }
        return false;
    }

    private Vector3 ClampToCameraRange(Vector3 desired, float maxDistance)
    {
        if (!_cam) _cam = Camera.main; if (!_cam) return desired;
        Vector3 camPos = _cam.transform.position;
        Vector3 delta = desired - camPos;
        float dist = delta.magnitude;
        return (dist > maxDistance) ? camPos + delta.normalized * maxDistance : desired;
    }

    private bool FindNearestFreeCellPhysics(Vector2Int start, float y, int maxRadius, out Vector2Int result)
    {
        if (!IsCellOccupiedPhysics(start, y)) { result = start; return true; }
        for (int r = 1; r <= maxRadius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                var c1 = new Vector2Int(start.x + dx, start.y + r); if (!IsCellOccupiedPhysics(c1, y)) { result = c1; return true; }
                var c2 = new Vector2Int(start.x + dx, start.y - r); if (!IsCellOccupiedPhysics(c2, y)) { result = c2; return true; }
            }
            for (int dz = -r + 1; dz <= r - 1; dz++)
            {
                var c3 = new Vector2Int(start.x - r, start.y + dz); if (!IsCellOccupiedPhysics(c3, y)) { result = c3; return true; }
                var c4 = new Vector2Int(start.x + r, start.y + dz); if (!IsCellOccupiedPhysics(c4, y)) { result = c4; return true; }
            }
        }
        result = start; return false;
    }

    private bool IsCellOccupiedPhysics(Vector2Int cell, float y)
    {
        Vector3 center = new Vector3(cell.x + 0.5f, y, cell.y + 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, occupancyRadius, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i]; if (!col) continue;

            // ignore tab ghost & preview
            if (_ghost && col.transform.IsChildOf(_ghost.transform)) continue;
            if (_visualPreview && col.transform.IsChildOf(_visualPreview.transform)) continue;

            var drag = col.GetComponentInParent<DraggableObject>(); if (!drag) continue;

            Vector3 p = drag.transform.position;
            Vector2Int other = new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.z));
            if (other == cell) return true;
        }
        return false;
    }

    private void RememberAndLockLayerRecursive(GameObject root, int toLayer)
    {
        var stack = new Stack<Transform>(); stack.Push(root.transform);
        while (stack.Count > 0)
        {
            var t = stack.Pop(); if (!t) continue;
            var go = t.gameObject;
            if (!_changedLayers.ContainsKey(go)) _changedLayers.Add(go, go.layer);
            go.layer = toLayer;
            for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
        }
    }

    private void MakeObjectSafe(GameObject root)
    {
        if (!forceSafeOnClones || !root) return;
        foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true)) if (mb) mb.enabled = false;
        foreach (var c in root.GetComponentsInChildren<Collider>(true)) if (c) c.enabled = false;
        foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true)) if (rb) { rb.isKinematic = true; rb.detectCollisions = false; }
        foreach (var a in root.GetComponentsInChildren<Animator>(true)) if (a) a.enabled = false;
        foreach (var a in root.GetComponentsInChildren<AudioSource>(true)) if (a) { a.Stop(); a.enabled = false; }
        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (!ps) continue; var main = ps.main; main.loop = false;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var em = ps.emission; em.enabled = false;
        }
    }

    private void SetLayerRecursive(GameObject root, int layer)
    {
        var stack = new Stack<Transform>(); stack.Push(root.transform);
        while (stack.Count > 0)
        {
            var t = stack.Pop(); if (!t) continue;
            t.gameObject.layer = layer;
            for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
        }
    }

    private void EnsureHandsRefs()
    {
        if (GrabbingGO == null)  GrabbingGO  = FindSceneObjectByName("Grabbing");
        if (HoldingGO == null)   HoldingGO   = FindSceneObjectByName("Holding");
        if (PointingGO == null)  PointingGO  = FindSceneObjectByName("Pointing");
        SetHandsForLock(_locked);
    }

    private void SetHandsForLock(bool lockOn)
    {
        if (HoldingGO)  HoldingGO.SetActive(lockOn);
        if (GrabbingGO) GrabbingGO.SetActive(!lockOn && DraggableObject.ActiveDragCount == 0);
        if (PointingGO && lockOn) PointingGO.SetActive(false);
    }

    private GameObject FindSceneObjectByName(string name)
    {
        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in all)
        {
            if (!go) continue;
            if (go.name != name) continue;
            if (!go.scene.IsValid()) continue;
            if (go.hideFlags != HideFlags.None) continue;
            return go;
        }
        return null;
    }

    // ======== PUBLIC so you can also hook it to UI buttons if desired ========
    public void ApplyLock(bool lockOn)
    {
        _locked = lockOn;
        BuildModeActive = lockOn;

        // Initialize placement height when entering build mode
        if (lockOn)
        {
            if (setInitialHeightFromSurface && TryGetAimPoint(out var aim))
                _placementY = Mathf.Clamp(aim.y, minHeight, maxHeight);
            else
                _placementY = Mathf.Clamp(_placementY, minHeight, maxHeight);
        }

        SetHandsForLock(lockOn); // immediate swap

        if (lockOn)
        {
            foreach (var d in EnumerateSceneDraggables())
            {
                if (!d) continue;
                RememberAndLockLayerRecursive(d.gameObject, _lockLayer);
            }
            EnsureGhostForCurrent();
            EnsurePreviewForCurrent();
        }
        else
        {
            foreach (var kvp in _changedLayers)
                if (kvp.Key) kvp.Key.layer = kvp.Value;
            _changedLayers.Clear();

            DestroyGhostIfAny();
            DestroyPreviewIfAny();
        }
    }

    private static List<DraggableObject> EnumerateSceneDraggables()
    {
        var res = new List<DraggableObject>();
        var all = Resources.FindObjectsOfTypeAll<DraggableObject>();
        foreach (var d in all)
        {
            if (!d) continue;
            var go = d.gameObject;
            if (!go || !go.scene.IsValid()) continue;
            if (go.hideFlags != HideFlags.None) continue;
            res.Add(d);
        }
        return res;
    }
}
