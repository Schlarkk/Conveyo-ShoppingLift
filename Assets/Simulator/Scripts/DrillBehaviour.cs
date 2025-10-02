using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillBehaviour : MonoBehaviour
{
    int activeDroppers = 0;
    bool poor;

    public GameObject prefabs;

    // Keep track of active droppers to disable energy when needed
    private readonly List<DraggableObject> droppersInRange = new List<DraggableObject>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dropper"))
        {
            DraggableObject DG = other.GetComponent<DraggableObject>();
            PrefabSpawner PS = other.gameObject.GetComponent<PrefabSpawner>();
            if (DG != null && PS != null)
            {
                if (!poor)
                {
                    DG.Material = true;
                    PS.prefabToSpawn = prefabs;
                    activeDroppers++;
                    droppersInRange.Add(DG);
                }
                else
                {
                    DG.Material = false;
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Dropper"))
        {
            DraggableObject DG = other.GetComponent<DraggableObject>();
            if (DG != null)
            {
                DG.Material = false;
                activeDroppers = Mathf.Max(0, activeDroppers - 1);
                droppersInRange.Remove(DG);
            }
        }
    }

void Update()
{
    if (!poor && activeDroppers > 0)
    {
        MoneyManager.GlobalMoney -= 0.001f * activeDroppers * Time.deltaTime;
    }

    if (MoneyManager.GlobalMoney <= 0 && !poor)
    {
        poor = true;
        foreach (var dg in droppersInRange)
        {
            if (dg != null) dg.Material = false;
        }
    }

    if (poor && MoneyManager.GlobalMoney > 0)
    {
        poor = false;
        foreach (var dg in droppersInRange)
        {
            if (dg != null) dg.Material = true;
        }
    }
}
}
