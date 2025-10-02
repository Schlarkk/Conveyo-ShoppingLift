using UnityEngine;

/// Attach to every placeable prefab (it can be auto-added at runtime).
/// Holds the prefab "Key" and captures transform state.
[DisallowMultipleComponent]
[RequireComponent(typeof(SaveableEntity))]
public class PlacedObject : MonoBehaviour, ISaveable
{
    [SerializeField] private string prefabKey;         // must match a key in PrefabRegistry (or use prefab.name)
    [SerializeField] private SaveableEntity entity;    // ensured at runtime

    public string PrefabKey => prefabKey;
    public SaveableEntity Entity => entity;

    // --- IMPORTANT: runtime guarantee so SaveManager never sees null Entity ---
    private void Awake()
    {
        if (entity == null)
        {
            entity = GetComponent<SaveableEntity>();
            if (entity == null) entity = gameObject.AddComponent<SaveableEntity>();
        }
    }

    private void Reset()
    {
        entity = GetComponent<SaveableEntity>();
        if (entity == null) entity = gameObject.AddComponent<SaveableEntity>();
    }

    private void OnValidate()
    {
        if (entity == null) entity = GetComponent<SaveableEntity>();
    }

    public void SetPrefabKey(string key) => prefabKey = key;

    [System.Serializable]
    private struct TransformState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    public object CaptureState()
    {
        return new TransformState
        {
            position = transform.position,
            rotation = transform.rotation,
            scale    = transform.localScale
        };
    }

    public void RestoreState(object state)
    {
        if (state is TransformState s)
        {
            transform.SetPositionAndRotation(s.position, s.rotation);
            transform.localScale = s.scale;
        }
    }
}
