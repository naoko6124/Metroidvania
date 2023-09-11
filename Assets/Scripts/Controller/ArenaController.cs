using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaController : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private GameObject door;
    [SerializeField] private GolemMovement golem;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            door.SetActive(true);
            golem.Activate();
            gameObject.SetActive(false);
        }
    }
}
