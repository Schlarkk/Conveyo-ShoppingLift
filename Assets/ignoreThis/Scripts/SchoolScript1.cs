using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SchoolScript1 : MonoBehaviour
{
    public GameObject Cube;
    public GameObject Sphere;
    public GameObject Capsule;
    public GameObject Cylinder;

    GameObject ActivateObject;

    public float rotationSpeed = 1.0f;

    private static readonly Vector3 SPAWN_POS = new Vector3(0f, 0.377f, -9.137f);
    private static readonly Quaternion SPAWN_ROT = Quaternion.identity;

    private void Update()
    {
        InputKeys();
    }

    void InputKeys()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            ActivateObject = Cube;
            SpawnWithRigidbody(ActivateObject, SPAWN_POS, SPAWN_ROT);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            ActivateObject = Sphere;
            SpawnWithRigidbody(ActivateObject, SPAWN_POS, SPAWN_ROT);
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            ActivateObject = Capsule;
            SpawnWithRigidbody(ActivateObject, SPAWN_POS, SPAWN_ROT);
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            ActivateObject = Cylinder;
            SpawnWithRigidbody(ActivateObject, SPAWN_POS, SPAWN_ROT);
        }


        if (Input.GetKey(KeyCode.A))
        {
            ActivateObject.transform.Rotate(0, -rotationSpeed, 0);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            ActivateObject.transform.Rotate(0, rotationSpeed, 0);
        }
        else if (Input.GetKey(KeyCode.W))
        {
            ActivateObject.transform.Rotate(rotationSpeed, 0, 0);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            ActivateObject.transform.Rotate(-rotationSpeed, 0, 0);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            ActivateObject.transform.Rotate(0, 0, -rotationSpeed);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            ActivateObject.transform.Rotate(0, 0, rotationSpeed);
        }

        if (Input.GetKey(KeyCode.Z))
        {
            ActivateObject.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
        }
        else if (Input.GetKey(KeyCode.X))
        {
            ActivateObject.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
        }
    }

    private static Rigidbody SpawnWithRigidbody(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[ShapeSpawner] Tried to spawn a null prefab.");
            return null;
        }

        GameObject go = Instantiate(prefab, pos, rot);
        Material material = go.GetComponent<MeshRenderer>().material;

        float r = Random.Range(0f,1f);
        float g = Random.Range(0f,1f);
        float b = Random.Range(0f,1f);
        Color randColor = new Color(r,g,b,1f);

        material.SetColor("_Color", randColor);

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody>();

            rb.mass = 1f;
            rb.drag = 0f;
            rb.angularDrag = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        return rb;
    }
}
