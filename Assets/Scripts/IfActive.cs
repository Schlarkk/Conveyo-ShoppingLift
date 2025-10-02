using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfActive : MonoBehaviour
{
    public GameObject firstObject;
    public GameObject secondObject;
    bool secondObjectWasActive;

    // Update is called once per frame
    void Update()
    {
        if (firstObject != null || secondObject != null)
        {
            if (secondObject.activeSelf)
            {
                secondObjectWasActive = true;
                if (firstObject.activeSelf)
                {
                    secondObject.SetActive(false);
                }
            }

            if (secondObjectWasActive && !firstObject.activeSelf)
            {
                secondObject.SetActive(true);
                secondObjectWasActive = false;
            }
            
        }
    }
}
