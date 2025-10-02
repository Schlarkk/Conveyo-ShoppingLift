using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class schoolScript2 : MonoBehaviour
{
    int character = 1;

    public GameObject character1;
    public GameObject character2;
    public GameObject character3;
    public GameObject character4;

    // Update is called once per frame
    void Update()
    {
        PlayerInput();
        characterselection();
    }

    void PlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (character >= 4)
            {
                return;
            }
            else
            {
                character++;
            }
        }


        if (Input.GetKeyDown(KeyCode.A))
        {
            if (character <= 1)
            {
                return;
            }
            else
            {
                character--;
            }
        }
    }

    void characterselection()
    {
        if (character == 1)
        {
            Debug.Log("Character 1 selected");

            character1.transform.position = new Vector3(0, 0, 0);
            character2.transform.position = new Vector3(10, 0, 0);
            character4.transform.position = new Vector3(10, 0, 0);
            character3.transform.position = new Vector3(10, 0, 0);
        }
        if (character == 2)
        {
            Debug.Log("Character 2 selected");

            character2.transform.position = new Vector3(0, 0, 0);
            character1.transform.position = new Vector3(10, 0, 0);
            character4.transform.position = new Vector3(10, 0, 0);
            character3.transform.position = new Vector3(10, 0, 0);
        }
        if (character == 3)
        {
            Debug.Log("Character 3 selected");

            character3.transform.position = new Vector3(0, 0, 0);
            character2.transform.position = new Vector3(10, 0, 0);
            character1.transform.position = new Vector3(10, 0, 0);
            character4.transform.position = new Vector3(10, 0, 0);
        }
        if (character == 4)
        {
            Debug.Log("Character 4 selected");

            character4.transform.position = new Vector3(0, 0, 0);
            character2.transform.position = new Vector3(10, 0, 0);
            character3.transform.position = new Vector3(10, 0, 0);
            character1.transform.position = new Vector3(10, 0, 0);
        }
    }

}
