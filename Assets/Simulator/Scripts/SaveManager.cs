using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Registry & Scene")]
    [SerializeField] private PrefabRegistry prefabRegistry;
    [SerializeField] private Transform dynamicRoot; // optional parent for placed instances
    public Transform DynamicRoot => dynamicRoot;

    [Header("Saving")]
    [SerializeField] private string saveFileName = "save.json";
    [SerializeField] private bool   autoSave     = true;
    [SerializeField] private float  autoSaveInterval = 60f;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = false;

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
    private const string SAVE_VERSION = "1.0.0";

    [Serializable]
    private class ComponentState
    {
        public string ownerType; // component type (AssemblyQualifiedName)
        public string stateType; // returned state type (AssemblyQualifiedName)
        public string json;      // JsonUtility of the state object
    }

    [Serializable]
    private class PlacedObjectData
    {
        public string prefabKey;
        public string id;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public List<ComponentState> components = new();
    }

    [Serializable]
    private class SaveGame
    {
        public string version = SAVE_VERSION;
        public float  money = 0f; // uses MoneyManager.GlobalMoney
        public List<PlacedObjectData> objects = new();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (autoSave) InvokeRepeating(nameof(AutoSave), autoSaveInterval, autoSaveInterval);
    }

    private void AutoSave()
    {
        try { Save(); }
        catch (Exception e) { Debug.LogWarning($"[Save] AutoSave failed: {e}"); }
    }

    public void Save()
    {
        var data = new SaveGame
        {
            money = MoneyManager.GlobalMoney
        };

        int found = 0, saved = 0, skipped = 0;

        var placed = FindObjectsByType<PlacedObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
        found = placed.Length;

        foreach (var po in placed)
        {
            if (po == null) { skipped++; continue; }

            // Ensure we have an entity even if PlacedObject was added at runtime
            var entity = po.Entity != null ? po.Entity : po.GetComponent<SaveableEntity>();
            if (entity == null) entity = po.gameObject.AddComponent<SaveableEntity>();

            var pod = new PlacedObjectData
            {
                prefabKey = !string.IsNullOrWhiteSpace(po.PrefabKey) ? po.PrefabKey : po.gameObject.name,
                id        = entity.Id,
                position  = po.transform.position,
                rotation  = po.transform.rotation,
                scale     = po.transform.localScale
            };

            // Capture all ISaveable components under this entity (excluding PlacedObject itself)
            foreach (var isav in entity.GetSaveables(includeInactive: true))
            {
                if (ReferenceEquals(isav, po)) continue; // we already captured transform

                var stateObj = isav.CaptureState();
                if (stateObj == null) continue;

                var ownerType = isav.GetType().AssemblyQualifiedName;
                var stateType = stateObj.GetType().AssemblyQualifiedName;
                var json      = JsonUtility.ToJson(stateObj);

                pod.components.Add(new ComponentState
                {
                    ownerType = ownerType,
                    stateType = stateType,
                    json      = json
                });
            }

            data.objects.Add(pod);
            saved++;
        }

        // Write file
        var dir = Path.GetDirectoryName(SavePath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var jsonText = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, jsonText);

        if (verboseLogs)
            Debug.Log($"[Save] Found={found}, Saved={saved}, Skipped={skipped}, Money=${data.money:0}. Path: {SavePath}");
        else
            Debug.Log($"[Save] Wrote {saved} objects + ${data.money:0} to {SavePath}");
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"[Save] No save found at {SavePath}");
            return;
        }

        var jsonText = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<SaveGame>(jsonText);
        if (data == null)
        {
            Debug.LogError("[Save] Failed to parse save json.");
            return;
        }

        // Money
        MoneyManager.GlobalMoney = data.money;

        // Clear current placed objects (only those with PlacedObject)
        var existing = FindObjectsByType<PlacedObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
        foreach (var po in existing)
        {
            if (po != null) Destroy(po.gameObject);
        }

        // Rebuild from save
        int spawned = 0;
        foreach (var pod in data.objects)
        {
            if (!prefabRegistry || !prefabRegistry.TryGet(pod.prefabKey, out var prefab) || prefab == null)
            {
                Debug.LogWarning($"[Save] Missing prefab for key '{pod.prefabKey}', skipping.");
                continue;
            }

            var parent = dynamicRoot != null ? dynamicRoot : null;
            var go = Instantiate(prefab, pod.position, pod.rotation, parent);
            go.transform.localScale = pod.scale;

            var po = go.GetComponent<PlacedObject>();
            if (po == null) po = go.AddComponent<PlacedObject>();
            po.SetPrefabKey(pod.prefabKey);

            // Ensure/save identity
            var entity = po.Entity != null ? po.Entity : go.GetComponent<SaveableEntity>();
            if (entity == null) entity = go.AddComponent<SaveableEntity>();
            entity.ForceSetId_ForLoad(pod.id);

            // Apply component states
            foreach (var cs in pod.components)
            {
                var ownerType = Type.GetType(cs.ownerType);
                var stateType = Type.GetType(cs.stateType);

                if (ownerType == null || stateType == null)
                {
                    Debug.LogWarning($"[Save] Type resolution failed (owner:{cs.ownerType}, state:{cs.stateType}).");
                    continue;
                }

                var compComponent = go.GetComponent(ownerType);
                if (compComponent is ISaveable isav)
                {
                    var stateObj = JsonUtility.FromJson(cs.json, stateType);
                    isav.RestoreState(stateObj);
                }
                else
                {
                    Debug.LogWarning($"[Save] Component '{ownerType}' not found or not ISaveable on '{go.name}'.");
                }
            }

            spawned++;
        }

        Debug.Log($"[Save] Loaded {spawned} placed objects, money=${MoneyManager.GlobalMoney:0}");
    }

    public void NewGame()
    {
        // Clears existing + deletes save file
        var existing = FindObjectsByType<PlacedObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
        foreach (var po in existing)
        {
            if (po != null) Destroy(po.gameObject);
        }

        MoneyManager.GlobalMoney = 10f;

        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Save] Failed to delete save: {e}");
        }

        Debug.Log("[Save] New game initialized.");
    }

    private void OnApplicationQuit()
    {
        try { Save(); }
        catch (Exception e) { Debug.LogWarning($"[Save] On quit save failed: {e}"); }
    }
}
