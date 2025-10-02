using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class IgnoreForPlayer : MonoBehaviour
{
    [Header("What to ignore")]
    [Tooltip("Only GameObjects that have this tag will be ignored. Their own colliders only (default).")]
    public string tagToIgnore = "IgnoreMe";

    [Tooltip("If ON, also ignore colliders on children of tagged objects. (Default OFF to avoid over-ignoring)")]
    public bool includeChildrenOfTagged = false;

    [Header("Refresh")]
    [Tooltip("Re-apply ignores periodically to catch late-spawned tagged objects.")]
    public bool autoRefresh = true;

    [Min(0.05f)]
    public float refreshInterval = 0.5f;

    [Header("Debug")]
    public bool verbose = true;

    private Collider[] selfColliders;
    private Coroutine refresher;

    // Track which pairs we disabled so we don't re-call every frame
    private readonly HashSet<int> appliedPairs = new HashSet<int>(); // orderless pair key

    // ===== Lifecycle =====
    private void Awake()
    {
        CacheSelfColliders();
    }

    private void OnEnable()
    {
        ApplyIgnores();
        if (autoRefresh) refresher = StartCoroutine(RefreshLoop());
    }

    private void OnDisable()
    {
        if (refresher != null) { StopCoroutine(refresher); refresher = null; }
        appliedPairs.Clear();
        // NOTE: We do not auto-restore pairs here because Unity doesn't ref-count IgnoreCollision.
        // If you need restoration, store pairs per-system and turn them back on explicitly.
    }

    private void CacheSelfColliders()
    {
        // Includes CharacterController because it inherits from Collider
        selfColliders = GetComponentsInChildren<Collider>(includeInactive: false);
        if ((selfColliders == null || selfColliders.Length == 0) && verbose)
        {
            Debug.LogWarning("[PreciseIgnoreByTag] No colliders found on this player (including CharacterController).");
        }
    }

    private IEnumerator RefreshLoop()
    {
        var wait = new WaitForSeconds(refreshInterval);
        while (true)
        {
            ApplyIgnores();
            yield return wait;
        }
    }

    // ===== Core =====
    private void ApplyIgnores()
    {
        if (selfColliders == null || selfColliders.Length == 0)
            CacheSelfColliders();

        if (selfColliders == null || selfColliders.Length == 0)
            return;

        GameObject[] tagged;
        try { tagged = GameObject.FindGameObjectsWithTag(tagToIgnore); }
        catch
        {
            Debug.LogError($"[PreciseIgnoreByTag] Tag '{tagToIgnore}' does not exist. Create it in Project Settings → Tags and Layers.");
            return;
        }

        int selfCount = 0, targetCount = 0, newPairs = 0;
        foreach (var c in selfColliders) if (c) selfCount++;

        foreach (var go in tagged)
        {
            if (!go) continue;

            Collider[] targetCols = includeChildrenOfTagged
                ? go.GetComponentsInChildren<Collider>(includeInactive: true)
                : go.GetComponents<Collider>(); // <-- only colliders on the tagged object itself

            if (targetCols == null || targetCols.Length == 0) continue;
            targetCount += targetCols.Length;

            foreach (var a in selfColliders)
            {
                if (!a) continue;

                foreach (var b in targetCols)
                {
                    if (!b) continue;
                    if (a == b) continue; // skip self

                    int key = MakePairKey(a.GetInstanceID(), b.GetInstanceID());
                    if (appliedPairs.Add(key))
                    {
                        Physics.IgnoreCollision(a, b, true);
                        newPairs++;
                        if (verbose) Debug.Log($"[PreciseIgnoreByTag] Ignoring {a.name} ↔ {b.name}");
                    }
                }
            }
        }

        if (verbose)
        {
            Debug.Log($"[PreciseIgnoreByTag] SelfColliders={selfCount}, TargetCollidersFound={targetCount}, NewIgnoresApplied={newPairs}, includeChildrenOfTagged={includeChildrenOfTagged}");
        }
    }

    private static int MakePairKey(int a, int b)
    {
        if (a > b) { int t = a; a = b; b = t; }
        unchecked { return (a * 73856093) ^ (b * 19349663); }
    }
}
