using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleUI : MonoBehaviour
{
    public void NextScene()
    {
        SceneManager.LoadScene(1);
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
