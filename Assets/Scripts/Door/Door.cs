using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool locked;

    public void True()
    {
        locked = false;
    }

    public void Unlock()
    {
        locked = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!locked)
            {
                Destroy(gameObject);
            }
        }
    }
}
