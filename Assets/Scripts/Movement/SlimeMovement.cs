using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SlimeMovement : MonoBehaviour
{
    [Header("Life")]
    public int lifePoints;

    [Header("Jump")]
    public Vector2 limitLeft;
    public Vector2 limitRight;
    public float jumpTime;
    public float jumpForce;
    private float timer = 0f;

    [Header("Collision")]
    public float collisionPower;
    private Shader shaderGUItext;
    private Shader shaderSpritesDefault;

    [Header("Drop")]
    [SerializeField] private GameObject expOrb;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    void Start()
    {
        shaderGUItext = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        timer = jumpTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0f)
        {
            if (transform.position.x < limitLeft.x)
                sprite.flipX = false;
            if (transform.position.x > limitRight.x)
                sprite.flipX = true;

            Jump();
            timer = jumpTime;
        }
    }

    private void Jump()
    {
        if (sprite.flipX)
            rb.AddForce((Vector2.left + Vector2.up) * jumpForce, ForceMode2D.Impulse);
        else
            rb.AddForce((Vector2.right + Vector2.up) * jumpForce, ForceMode2D.Impulse);
        anim.SetTrigger("Jump");
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
            Instantiate(expOrb, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
