using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceBehaviour : MonoBehaviour
{
    public string targetTag = "Poop";          // Only destroy objects with this tag
    public float destroyDelay = 0.5f;             // Delay before destruction
    public string intScriptName = "ItemValue"; // Script name on target with int
    public string intFieldName = "Value";

    public float howmuch = 1;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        if (other.CompareTag(targetTag))
        {
            // Try to get the script by name using reflection
            Component script = other.GetComponent(intScriptName);

            if (script != null)
            {
                var field = script.GetType().GetField(intFieldName);

                if (field != null && field.FieldType == typeof(float))
                {
                    float value = (float)field.GetValue(script);
                    Debug.Log($"Collided with {other.name}, int value: {value}");

                    MoneyManager.GlobalMoney += value * howmuch;

                    StartCoroutine(DestroyAfterDelay(other));
                }
                else
                {
                    Debug.LogWarning($"Field '{intFieldName}' not found or not an int in script '{intScriptName}'");
                }
            }
            else
            {
                Debug.LogWarning($"Script '{intScriptName}' not found on '{other.name}'");
            }
        }
    }

    IEnumerator DestroyAfterDelay(GameObject target)
    {
        yield return new WaitForSeconds(destroyDelay);
        if (target != null)
            Destroy(target);
    }
}
