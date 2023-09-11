using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GolemMovement : MonoBehaviour
{
    [Header("Life")]
    public int lifePoints;

    [Header("Move")]
    public float moveDelay;
    public float moveTime;
    public float moveSpeed;
    private float timer = 0f;

    [Header("Attack")]
    public float attackDistance;
    public float bulletSpeed;
    public float pentagramDuration;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject pentagram;

    [Header("Collision")]
    public float collisionPower;
    private Shader shaderGUItext;
    private Shader shaderSpritesDefault;

    [Header("Drop")]
    [SerializeField] private GameObject expOrb;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private Transform target;
    private int moveCount = 0;

    private bool active = false;

    public void Activate()
    {
        active = true;
    }

    void Start()
    {
        shaderGUItext = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        target = GameObject.FindWithTag("Player").transform;

        timer = moveDelay;
    }

    void Update()
    {
        if (!active) return;

        timer -= Time.deltaTime;

        if (timer < 0f)
        {
            timer = 0f;

            if (moveCount == 2)
            {
                float distance = Vector2.Distance(target.transform.position, transform.position);
                if (distance > attackDistance)
                {
                    StartCoroutine(RangedAttack());
                    timer += 1f;
                }
                else
                {
                    StartCoroutine(MeleeAttack());
                    timer += 1f + pentagramDuration;
                }
                moveCount = 0;
            }
            else
            {
                Vector2 direction = target.transform.position - transform.position;
                Vector2 moveDir = new Vector3(Mathf.Ceil(direction.x), Mathf.Ceil(direction.y), 0f);
                StartCoroutine(Move(moveDir.normalized));
                moveCount++;
            }

            timer += moveDelay;
        }
    }
    private IEnumerator RangedAttack()
    {
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(1);
        SpawnBullet(2f, 0f);
        SpawnBullet(-2f, 0f);
        SpawnBullet(0f, 2f);
        SpawnBullet(0f, -2f);
        SpawnBullet(1.5f, 1.5f);
        SpawnBullet(-1.5f, 1.5f);
        SpawnBullet(1.5f, -1.5f);
        SpawnBullet(-1.5f, -1.5f);
    }

    private IEnumerator MeleeAttack()
    {
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(1);
        pentagram.SetActive(true);
        yield return new WaitForSeconds(pentagramDuration);
        pentagram.SetActive(false);
    }

    private void SpawnBullet(float x, float y)
    {
        GameObject gb = Instantiate(bullet, transform.position + new Vector3(x, y, 0f), Quaternion.identity);
        gb.GetComponent<Rigidbody2D>().velocity = new Vector2(x, y) * bulletSpeed;
    }

    private IEnumerator Move(Vector2 direction)
    {
        rb.velocity = direction * moveSpeed;
        anim.SetBool("Running", true);
        yield return new WaitForSeconds(moveTime);
        rb.velocity = new Vector2(0f, 0f);
        anim.SetBool("Running", false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Attack"))
        {
            float damageDirection = 1f;
            if (collision.transform.position.x > transform.position.x)
                damageDirection = -1f;
            StartCoroutine(TakeDamage(damageDirection));
        }
    }

    private IEnumerator TakeDamage(float direction)
    {
        lifePoints--;
        rb.velocity = Vector2.zero;
        rb.AddForce(collisionPower * new Vector2(direction, 0.5f), ForceMode2D.Impulse);
        sprite.material.shader = shaderGUItext;
        yield return new WaitForSeconds(0.5f);
        sprite.material.shader = shaderSpritesDefault;

        if (lifePoints <= 0)
        {
            for (int i = 0; i < 100; i++)
            {
                Instantiate(expOrb, transform.position, Quaternion.identity);
            }
            target.GetComponent<CharacterMovement>().Win();
            Destroy(gameObject);
        }
    }
}
