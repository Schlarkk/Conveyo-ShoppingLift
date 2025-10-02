using UnityEngine;

public class CollectedItem : MonoBehaviour
{
    public enum MaterialType
    {
        OilCube,
        Coal,
        Copper,
        Silver,
        Gold,
        Diamond,   // note: standard spelling
        Amethyst,
        Comet
    }

    [Header("Collected Item Settings")]
    public MaterialType Material = MaterialType.OilCube;

    [Tooltip("How many units this object contributes when collected.")]
    public long Amount = 1;

    // Optional: if true, this item can only be collected once, guarding duplicate collisions
    [HideInInspector] public bool Consumed = false;
}
