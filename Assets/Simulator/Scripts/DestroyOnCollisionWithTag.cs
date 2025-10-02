using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollisionWithTag : MonoBehaviour
{
    public string targetTag = "Ground"; // The tag to check collision against
    public float delay = 20f;

    private bool isColliding = false;
    private Coroutine destroyCoroutine;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(targetTag) && !isColliding)
        {
            isColliding = true;
            destroyCoroutine = StartCoroutine(DestroyAfterDelay());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(targetTag) && isColliding)
        {
            isColliding = false;
            if (destroyCoroutine != null)
            {
                StopCoroutine(destroyCoroutine);
                destroyCoroutine = null;
            }
        }
    }

    private System.Collections.IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}

