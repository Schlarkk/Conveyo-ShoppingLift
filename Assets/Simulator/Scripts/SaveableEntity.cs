using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-200)] // ensure ID exists early
public class SaveableEntity : MonoBehaviour
{
    [SerializeField] private string id; // GUID
    public string Id => id;

    private void Awake()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString("N");
    }

    /// Used by SaveManager during load to preserve original identity.
    public void ForceSetId_ForLoad(string newId)
    {
        id = newId;
    }

    public IEnumerable<ISaveable> GetSaveables(bool includeInactive = true)
    {
        return GetComponentsInChildren<ISaveable>(includeInactive);
    }
}
