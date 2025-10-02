using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Prefab Registry", fileName = "PrefabRegistry")]
public class PrefabRegistry : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string Key;        // e.g., "ConveyorStraight", "AssemblerT1"
        public GameObject Prefab; // Prefab with PlacedObject + SaveableEntity
    }

    [SerializeField] private List<Entry> entries = new();
    private Dictionary<string, GameObject> map;

    private void OnEnable()
    {
        map = new Dictionary<string, GameObject>(entries.Count);
        foreach (var e in entries)
        {
            if (!string.IsNullOrWhiteSpace(e.Key) && e.Prefab != null)
                map[e.Key] = e.Prefab;
        }
    }

    public bool TryGet(string key, out GameObject prefab)
    {
        if (map == null) OnEnable();
        return map.TryGetValue(key, out prefab);
    }
}
