using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XpMagnet : MonoBehaviour
{
    [Header("Magnet")]
    public float magnetRadius;
    public float magnetSpeed;

    private Transform target;

    void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector2.Distance(target.transform.position, transform.position);
        if (distance < magnetRadius)
        {
            Vector2 direction = target.transform.position - transform.position;
            Vector3 moveDir = new Vector3(Mathf.Ceil(direction.x), Mathf.Ceil(direction.y), 0f);
            transform.position += moveDir.normalized * Time.deltaTime * magnetSpeed;
        }
    }
}
