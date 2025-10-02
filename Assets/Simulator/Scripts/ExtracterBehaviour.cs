using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtracterBehaviour : MonoBehaviour
{
    int activeDroppers = 0;
    bool poor;

    // Keep track of active droppers to disable energy when needed
    private readonly List<DraggableObject> droppersInRange = new List<DraggableObject>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dropper"))
        {
            DraggableObject DG = other.GetComponent<DraggableObject>();
            if (DG != null)
            {
                if (!poor)
                {
                    DG.Energy = true;
                    activeDroppers++;
                    droppersInRange.Add(DG);
                }
                else
                {
                    DG.Energy = false;
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
                DG.Energy = false;
                activeDroppers = Mathf.Max(0, activeDroppers - 1);
                droppersInRange.Remove(DG);
            }
        }
    }

void Update()
{
    if (!poor && activeDroppers > 0)
    {
        MoneyManager.GlobalMoney -= 0.08f * activeDroppers * Time.deltaTime;
    }

    if (MoneyManager.GlobalMoney <= 0 && !poor)
    {
        poor = true;
        foreach (var dg in droppersInRange)
        {
            if (dg != null) dg.Energy = false;
        }
    }

    if (poor && MoneyManager.GlobalMoney > 0)
    {
        poor = false;
        foreach (var dg in droppersInRange)
        {
            if (dg != null) dg.Energy = true;
        }
    }
}
}
