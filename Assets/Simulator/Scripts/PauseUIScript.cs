using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseUIScript : MonoBehaviour
{

    public GameObject PauseUIWhole;
    public GameObject MainUI;
    public GameObject TipUI;
    public GameObject Tip;

    bool UI;


    public AudioSource AS;
    public AudioClip UisoundOne;
    public AudioClip UisoundTwo;
    public AudioClip UisoundThree;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && UI)
        {
            UI = false;
            PauseUIWhole.SetActive(false);
            MainUI.SetActive(true);
            TipUI.SetActive(false);

            AS.clip = UisoundThree;    
            AS.Play();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
        else if (Input.GetKeyDown(KeyCode.Escape) && !UI)
        {
            UI = true;
            PauseUIWhole.SetActive(true);
            MainUI.SetActive(true);
            TipUI.SetActive(false);
            Tip.SetActive(false);

            AS.clip = UisoundThree;    
            AS.Play();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
    }

    public void Resume()
    {
        UI = false;
        PauseUIWhole.SetActive(false);
        MainUI.SetActive(true);
        TipUI.SetActive(false);

        AS.clip = UisoundTwo;    
        AS.Play();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    public void Tips()
    {
        MainUI.SetActive(false);
        TipUI.SetActive(true);

        AS.clip = UisoundOne;    
        AS.Play();
    }

    public void Back()
    {
        MainUI.SetActive(true);
        TipUI.SetActive(false);

        AS.clip = UisoundTwo;    
        AS.Play();
    }

    public void Return()
    {
        AS.clip = UisoundTwo;    
        AS.Play();


        SceneManager.LoadScene(0);
    }
    

}
