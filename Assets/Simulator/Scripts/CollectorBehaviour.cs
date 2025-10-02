using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectorBehaviour : MonoBehaviour
{
    [Header("Filtering")]
    [Tooltip("Only objects with this tag are eligible for collection.")]
    public string collectibleTag = "Poop";

    [Header("Timing")]
    public float destroyDelay = 0.25f;

    [Header("Debug")]
    public bool logCollections = false;

    private void Reset()
    {
        // Encourage trigger-based collection by default (optional)
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    // Support both physics and trigger styles
    private void OnCollisionEnter(Collision collision)
    {
        TryCollect(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other.gameObject);
    }

    private void TryCollect(GameObject other)
    {
        if (!other || !other.CompareTag(collectibleTag)) return;

        var item = other.GetComponent<CollectedItem>();
        if (item == null)
        {
            if (logCollections)
                Debug.LogWarning($"[Collector] '{other.name}' has tag '{collectibleTag}' but no CollectedItem component.");
            return;
        }

        if (item.Consumed) return; // already processed

        // FIX: Clamp to 0 using System.Math for long values
        long amount = Math.Max(0L, item.Amount);
        if (amount == 0)
        {
            if (logCollections)
                Debug.Log($"[Collector] '{other.name}' has zero Amount. Skipping.");
            return;
        }

        // Tally into the global inventory
        if (MaterialsInventory.Instance != null)
        {
            MaterialsInventory.Instance.Add(item.Material, amount);
            if (logCollections)
            {
                Debug.Log($"[Collector] Collected {amount}x {item.Material} from '{other.name}'. " +
                          $"Total now: {MaterialsInventory.Instance.Get(item.Material)}");
            }
        }
        else
        {
            Debug.LogError("[Collector] No MaterialsInventory instance found in scene!");
        }

        item.Consumed = true;

        // Remove the item
        if (destroyDelay <= 0f)
        {
            Destroy(other);
        }
        else
        {
            StartCoroutine(DestroyAfterDelay(other, destroyDelay));
        }
    }

    private IEnumerator DestroyAfterDelay(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null) Destroy(target);
    }
}
