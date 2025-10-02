using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathScreenActivation : MonoBehaviour
{
    public Animator anim;
    public Animator anim2;
    public GameObject anim2l;
    public FirstPersonController fps;

    void Start()
    {
        anim2 = anim2l.GetComponent<Animator>();
    }

    public void Death()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        fps.enabled = false;

    }

    public void PreDeath()
    {
        anim.SetTrigger("death");
        anim2.SetTrigger("death");
    }

    public void EscapeDeath()
    {
        anim.SetTrigger("no");
        anim2.SetTrigger("no");
    }
}
