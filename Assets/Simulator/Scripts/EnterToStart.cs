using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterToStart : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }       
    }
}
