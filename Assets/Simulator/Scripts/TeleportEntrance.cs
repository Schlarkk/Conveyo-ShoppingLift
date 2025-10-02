using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportEntrance : MonoBehaviour
{
    public int targetTeleportID = 0;
    public string teleportTag = "TeleportExit";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Poop")) // Or whatever tag your item uses
        {
            Teleport(other.gameObject);
        }
    }

    private void Teleport(GameObject objectToTeleport)
    {
        // Find all objects with the given tag
        GameObject[] targets = GameObject.FindGameObjectsWithTag(teleportTag);

        foreach (GameObject target in targets)
        {
            TeleportExit tp = target.GetComponent<TeleportExit>();
            if (tp != null && tp.teleportID == targetTeleportID)
            {
                // Move the incoming object to the matching exit
                objectToTeleport.transform.position = tp.transform.position;
                return;
            }
        }

        Debug.LogWarning("Teleport target with ID " + targetTeleportID + " not found.");
    }
}
