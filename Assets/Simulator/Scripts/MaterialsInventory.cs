using System;
using System.IO;
using UnityEngine;

public class MaterialsInventory : MonoBehaviour
{
    public static MaterialsInventory Instance { get; private set; }

    [Serializable]
    private class MaterialsSaveData
    {
        public long OilCube;
        public long Coal;
        public long Copper;
        public long Silver;
        public long Gold;
        public long Diamond;
        public long Amethyst;
        public long Comet;
    }

    // Backing fields
    [SerializeField] private long oilCube;
    [SerializeField] private long coal;
    [SerializeField] private long copper;
    [SerializeField] private long silver;
    [SerializeField] private long gold;
    [SerializeField] private long diamond;
    [SerializeField] private long amethyst;
    [SerializeField] private long comet;

    public event Action OnInventoryChanged;

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "MaterialsInventory.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    // --- Public getters ---
    public long Get(MaterialFromEnum mat)
    {
        switch (mat)
        {
            case MaterialFromEnum.OilCube:  return oilCube;
            case MaterialFromEnum.Coal:     return coal;
            case MaterialFromEnum.Copper:   return copper;
            case MaterialFromEnum.Silver:   return silver;
            case MaterialFromEnum.Gold:     return gold;
            case MaterialFromEnum.Diamond:  return diamond;
            case MaterialFromEnum.Amethyst: return amethyst;
            case MaterialFromEnum.Comet:    return comet;
            default: return 0;
        }
    }

    public long Get(CollectedItem.MaterialType mat)
        => Get((MaterialFromEnum)mat);

    // --- Public add method ---
    public void Add(CollectedItem.MaterialType mat, long amount)
    {
        if (amount == 0) return;

        switch (mat)
        {
            case CollectedItem.MaterialType.OilCube:  oilCube  += amount; break;
            case CollectedItem.MaterialType.Coal:     coal     += amount; break;
            case CollectedItem.MaterialType.Copper:   copper   += amount; break;
            case CollectedItem.MaterialType.Silver:   silver   += amount; break;
            case CollectedItem.MaterialType.Gold:     gold     += amount; break;
            case CollectedItem.MaterialType.Diamond:  diamond  += amount; break;
            case CollectedItem.MaterialType.Amethyst: amethyst += amount; break;
            case CollectedItem.MaterialType.Comet:    comet    += amount; break;
        }

        Save();
        OnInventoryChanged?.Invoke();
    }

    // --- Optional: Reset all counts ---
    public void ResetAll(bool saveAfter = true)
    {
        oilCube = coal = copper = silver = gold = diamond = amethyst = comet = 0;
        if (saveAfter) Save();
        OnInventoryChanged?.Invoke();
    }

    // --- Save / Load ---
    public void Save()
    {
        try
        {
            var data = new MaterialsSaveData
            {
                OilCube  = oilCube,
                Coal     = coal,
                Copper   = copper,
                Silver   = silver,
                Gold     = gold,
                Diamond  = diamond,
                Amethyst = amethyst,
                Comet    = comet
            };

            var json = JsonUtility.ToJson(data, prettyPrint: true);
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            File.WriteAllText(SavePath, json);
            // Debug.Log($"[MaterialsInventory] Saved to {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[MaterialsInventory] Save failed: {e}");
        }
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                // First run: start fresh
                ResetAll(saveAfter: true);
                return;
            }

            var json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<MaterialsSaveData>(json);
            if (data == null)
            {
                Debug.LogWarning("[MaterialsInventory] Invalid save file, resetting.");
                ResetAll(saveAfter: true);
                return;
            }

            oilCube  = data.OilCube;
            coal     = data.Coal;
            copper   = data.Copper;
            silver   = data.Silver;
            gold     = data.Gold;
            diamond  = data.Diamond;
            amethyst = data.Amethyst;
            comet    = data.Comet;

            OnInventoryChanged?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[MaterialsInventory] Load failed: {e}");
            // Fallback to zeros so the game stays playable
            ResetAll(saveAfter: true);
        }
    }

    // Bridge enum so the public getter can accept either enum type
    public enum MaterialFromEnum
    {
        OilCube,
        Coal,
        Copper,
        Silver,
        Gold,
        Diamond,
        Amethyst,
        Comet
    }
}
