using UnityEngine;

public class SaveHotkeys : MonoBehaviour
{
    [SerializeField] private KeyCode saveKey = KeyCode.F5;
    [SerializeField] private KeyCode loadKey = KeyCode.F9;
    [SerializeField] private KeyCode newKey  = KeyCode.F1;

    private void Update()
    {
        if (Input.GetKeyDown(saveKey))  { if (SaveManager.Instance) SaveManager.Instance.Save(); }
        if (Input.GetKeyDown(loadKey))  { if (SaveManager.Instance) SaveManager.Instance.Load(); }
        if (Input.GetKeyDown(newKey))   { if (SaveManager.Instance) SaveManager.Instance.NewGame(); }
    }
}
