using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterMovement : MonoBehaviour
{
    [Header("Stats")]
    public int level;
    public int life;
    public int exp;
    [SerializeField] private Slider lifeSlider;
    [SerializeField] private Slider expSlider;

    [Header("Unlockables")]
    public bool slide;
    public bool bible;

    [Header("Movement")]
    public float moveSpeed;
    public float accelRate;
    public float frictionAmount;
    [SerializeField] private Vector2 moveInput;

    [Header("Jump")]
    public float jumpForce;
    public float defaultGravity;
    public float fallGravity;
    [SerializeField] private float lastGroundedTime = 0f;
    [SerializeField] private float lastJumpTime = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    [Header("Dash")]
    public float dashSpeed;
    public float dashTimer;
    public float facingDirection { get; private set; } = 1f;
    private float lastDashTime = 0f;

    [Header("Attack")]
    [SerializeField] private GameObject attackSquare;
    [SerializeField] private GameObject bibleItem;
    private bool isAttacking;
    private float bibleTimer = 0f;

    [Header("Collision")]
    public float collisionPower;
    private Shader shaderGUItext;
    private Shader shaderSpritesDefault;

    [Header("GUI")]
    public GameObject pauseMenu;
    public GameObject slidePopup;
    public GameObject biblePopup;

    public CharacterInputActions actions;
    private Rigidbody2D rb;
    private BoxCollider2D hitbox;
    private Animator anim;
    private SpriteRenderer sprite;

    private void Start()
    {
        // Load Save

        slide = PlayerPrefs.GetInt("UnlockSlide", 0) == 1 ? true : false;
        bible = PlayerPrefs.GetInt("UnlockBible", 0) == 1 ? true : false;
        level = PlayerPrefs.GetInt("Level", 0);
        exp = PlayerPrefs.GetInt("Exp", 0);
        expSlider.value = exp;
        life = PlayerPrefs.GetInt("Life", 10);
        lifeSlider.value = life;

        // Shaders

        shaderGUItext = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");

        // Components

        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        anim = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        // Input Action Map

        actions = new CharacterInputActions();
        actions.Enable();

        actions.Character.Jump.performed += Jump;
        actions.Character.Jump.canceled += JumpCut;
        
        actions.Character.Dash.performed += Dash;

        actions.Character.Attack.performed += Attack;
        actions.Character.Pause.performed += Pause;
        actions.Character.Slide.performed += Slide;
    }

    private void Update()
    {
        // Input and Direction

        moveInput = actions.Character.Movement.ReadValue<Vector2>();
        if (moveInput.x > 0.1f) facingDirection = 1f;
        if (moveInput.x < -0.1f) facingDirection = -1f;

        // Freeze at Attack
        if (isAttacking) moveInput = Vector2.zero;
        attackSquare.transform.position = new Vector3(0.6f * facingDirection, 1f, 0f) + transform.position;

        // Ground Check

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, groundLayer);

        // Timers

        lastDashTime -= Time.deltaTime;
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;
        if (isGrounded) lastGroundedTime = 0f;

        // Animation

        if (facingDirection == 1f)
        {
            sprite.flipX = false;

            if (rb.velocity.x > 0.1f)
            {
                anim.SetBool("Running", true);
                anim.SetBool("Backing", false);
            }
            else if (rb.velocity.x < -0.1f)
            {
                anim.SetBool("Backing", true);
                anim.SetBool("Running", false);
            }
            else
            {
                anim.SetBool("Running", false);
                anim.SetBool("Backing", false);
            }
        }
        else if (facingDirection == -1f)
        {
            sprite.flipX = true;

            if (rb.velocity.x < -0.1f)
            {
                anim.SetBool("Running", true);
                anim.SetBool("Backing", false);
            }
            else if (rb.velocity.x > 0.1f)
            {
                anim.SetBool("Backing", true);
                anim.SetBool("Running", false);
            }
            else
            {
                anim.SetBool("Running", false);
                anim.SetBool("Backing", false);
            }
        }

        if (bible)
        {
            bibleItem.SetActive(true);
            bibleItem.transform.position = transform.position + new Vector3(Mathf.Cos(bibleTimer), Mathf.Sin(bibleTimer) + 0.5f, 0f) * 2f;
            bibleTimer += Time.deltaTime * 3f;
        }
    }

    private void FixedUpdate()
    {
        // Movement

        float desiredSpeed = moveInput.x * moveSpeed;

        float speedDif = desiredSpeed - rb.velocity.x;
        float movement = speedDif * accelRate;
        
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        // Friction

        if (Mathf.Abs(moveInput.x) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
            amount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }

        // Fall Gravity

        if (rb.velocity.y < 0f)
            rb.gravityScale = fallGravity;
        else
            rb.gravityScale = defaultGravity;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (biblePopup.activeInHierarchy)
        {
            biblePopup.SetActive(false);
            Time.timeScale = 1;
            return;
        }
        if (lastGroundedTime > lastJumpTime || isGrounded)
        {
            float force = jumpForce;
            if (rb.velocity.y < 0f)
                force -= rb.velocity.y;

            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
            lastJumpTime = 0.1f;
        }
    }

    private void JumpCut(InputAction.CallbackContext context)
    {
        if (rb.velocity.y > 0f) rb.velocity = new Vector2(rb.velocity.x, 0f);
    }

    private void Dash(InputAction.CallbackContext context)
    {
        if (lastDashTime < 0f && isGrounded)
        {
            rb.AddForce(dashSpeed * new Vector2(-facingDirection, 0f), ForceMode2D.Impulse);
            lastDashTime = dashTimer;
        }
    }

    private void Attack(InputAction.CallbackContext context)
    {
        if (!isAttacking && isGrounded)
        {
            StartCoroutine(Attacking());
        }
    }

    private IEnumerator Attacking()
    {
        isAttacking = true;
        anim.SetTrigger("Attacking");
        yield return new WaitForSeconds(0.1f);
        attackSquare.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        attackSquare.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        isAttacking = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            float damageDirection = 1f;
            if (collision.transform.position.x > transform.position.x)
                damageDirection = -1f;
            StartCoroutine(TakeDamage(damageDirection));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Exp"))
        {
            exp++;
            expSlider.value = exp;
            Destroy(collision.gameObject);

            if (exp == 4)
            {
                UpgradeLevel();
            }
        }
        if (collision.CompareTag("Death"))
        {
            actions.Disable();
            SceneManager.LoadScene("GameOver");
        }
    }

    private void UpgradeLevel()
    {
        exp = 0;
        expSlider.value = exp;
        PlayerPrefs.SetInt("Exp", 0);

        level++;
        PlayerPrefs.SetInt("Level", level);

        switch (level)
        {
            case 1:
                slidePopup.SetActive(true);
                Time.timeScale = 0;
                PlayerPrefs.SetInt("UnlockSlide", 1);
                slide = true;
                break;
            case 2:
                biblePopup.SetActive(true);
                Time.timeScale = 0;
                PlayerPrefs.SetInt("UnlockBible", 1);
                bible = true;
                break;
        }
    }

    private IEnumerator TakeDamage(float direction)
    {
        life -= 1;
        lifeSlider.value = life;

        rb.velocity = Vector2.zero;
        rb.AddForce(collisionPower * new Vector2(direction, 0.5f), ForceMode2D.Impulse);
        sprite.material.shader = shaderGUItext;
        yield return new WaitForSeconds(0.5f);
        sprite.material.shader = shaderSpritesDefault;

        if (life <= 0)
        {
            actions.Disable();
            SceneManager.LoadScene("GameOver");
        }
    }

    private void Pause(InputAction.CallbackContext context)
    {
        if (pauseMenu.activeInHierarchy)
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
        }
        else
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    private void Slide(InputAction.CallbackContext context)
    {
        if (slidePopup.activeInHierarchy)
        {
            slidePopup.SetActive(false);
            Time.timeScale = 1;
            return;
        }

        if (lastDashTime < 0f && isGrounded && slide)
        {
            rb.AddForce(dashSpeed * new Vector2(facingDirection, 0f), ForceMode2D.Impulse);
            lastDashTime = dashTimer;
            StartCoroutine(Sliding());
        }
    }

    private IEnumerator Sliding()
    {
        anim.SetBool("Sliding", true);
        isAttacking = true;
        hitbox.offset = new Vector2(0f, 0.4f);
        hitbox.size = new Vector2(1.4f, 0.8f);
        yield return new WaitForSeconds(0.5f);
        anim.SetBool("Sliding", false);
        isAttacking = false;
        hitbox.offset = new Vector2(0f, 0.9f);
        hitbox.size = new Vector2(0.8f, 1.8f);
    }

    public void Win()
    {
        StartCoroutine(DelayWin());
    }
    private IEnumerator DelayWin()
    {
        yield return new WaitForSeconds(5f);
        actions.Disable();
        SceneManager.LoadScene("YouWin");
    }
}
