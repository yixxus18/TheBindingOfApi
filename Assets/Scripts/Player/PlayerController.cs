using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Movement")]
    private Vector2 movementInput;
    public bool canMove = true;
    public int facingDirection = 1;
    private bool isKnockedback = false;

    [Header("Combat")]
    public Transform attackPoint;
    public LayerMask enemyLayer;
    public float attackCooldown = 0.5f;
    private float attackTimer = 0f;
    private bool hasDealtDamageThisAttack = false;
    private bool isAttacking = false;
    private HashSet<int> enemiesHitThisAttack = new HashSet<int>();

    [Header("Dodge")]
    public float dodgeSpeed = 15f;
    public float dodgeDuration = 0.2f;
    public float dodgeCooldown = 1f;
    private float dodgeTimer = 0f;
    private bool isDodging = false;

    [Header("Door Transitions")]
    private bool isTransitioning = false;

    [Header("Health UI")]
    public TMP_Text healthText;
    public Animator healthTextAnim;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        UpdateHealthUI();
    }

    void Update()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (dodgeTimer > 0)
            dodgeTimer -= Time.deltaTime;

        if (isKnockedback || isDodging)
        {
            movementInput = Vector2.zero;
            animator.SetFloat("horizontal", 0);
            animator.SetFloat("vertical", 0);
            return;
        }

        if (canMove)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            movementInput = new Vector2(moveX, moveY).normalized;
            animator.SetFloat("horizontal", Mathf.Abs(moveX));
            animator.SetFloat("vertical", Mathf.Abs(moveY));

            if (moveX > 0 && transform.localScale.x < 0 || moveX < 0 && transform.localScale.x > 0)
                Flip();
        }
        else
        {
            movementInput = Vector2.zero;
            animator.SetFloat("horizontal", 0);
            animator.SetFloat("vertical", 0);
        }

        if (Input.GetKeyDown(KeyCode.K) && attackTimer <= 0 && canMove && !isKnockedback && !isAttacking)
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.J) && dodgeTimer <= 0 && canMove && !isKnockedback)
        {
            StartCoroutine(Dodge());
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void FixedUpdate()
    {
        if (isKnockedback || isDodging)
            return;

        if (canMove && StatsManager.instance != null)
        {
            rb.linearVelocity = movementInput * StatsManager.instance.speed;
        }
    }

    void Attack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;
        animator.SetBool("isAttacking", true);
        hasDealtDamageThisAttack = false;
        enemiesHitThisAttack.Clear();
    }

    public void DealDamage()
    {
        if (hasDealtDamageThisAttack || attackPoint == null || StatsManager.instance == null)
            return;

        hasDealtDamageThisAttack = true;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, 1.5f, enemyLayer);

        foreach (var enemy in enemies)
        {
            int enemyID = enemy.gameObject.GetInstanceID();

            if (enemiesHitThisAttack.Contains(enemyID))
                continue;

            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(StatsManager.instance.power);
                enemyController.Knockback(transform, 5f, 0.2f, 0.3f);
                enemiesHitThisAttack.Add(enemyID);
            }
        }
    }

    public void FinishAttacking()
    {
        animator.SetBool("isAttacking", false);
        isAttacking = false;
        enemiesHitThisAttack.Clear();
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    IEnumerator Dodge()
    {
        isDodging = true;
        dodgeTimer = dodgeCooldown;
        Vector2 dodgeDirection = movementInput.magnitude > 0.1f ? movementInput : Vector2.right * facingDirection;
        rb.linearVelocity = dodgeDirection * dodgeSpeed;
        yield return new WaitForSeconds(dodgeDuration);
        isDodging = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void TakeDamage(int amount)
    {
        if (StatsManager.instance == null)
            return;

        StatsManager.instance.TakeDamage(amount);
        UpdateHealthUI();

        if (PlayerUIManager.instance != null)
            PlayerUIManager.instance.UpdateHealthBar();

        if (StatsManager.instance.currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (StatsManager.instance == null)
            return;

        StatsManager.instance.Heal(amount);
        UpdateHealthUI();

        if (PlayerUIManager.instance != null)
            PlayerUIManager.instance.UpdateHealthBar();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null && StatsManager.instance != null)
        {
            healthText.text = $"HP: {StatsManager.instance.currentHealth}/{StatsManager.instance.maxHealth}";

            if (healthTextAnim != null)
                healthTextAnim.Play("TextUpdate", -1, 0f);
        }
    }

    void Die()
    {
        animator.SetTrigger("Death");
        canMove = false;
    }

    public void Knockback(Transform enemy, float force, float stunTime)
    {
        if (isKnockedback) return;
        StartCoroutine(KnockbackCoroutine(enemy, force, stunTime));
    }

    IEnumerator KnockbackCoroutine(Transform enemy, float force, float stunTime)
    {
        isKnockedback = true;
        canMove = false;
        Vector2 direction = (transform.position - enemy.position).normalized;
        rb.linearVelocity = direction * force;
        yield return new WaitForSeconds(stunTime);
        rb.linearVelocity = Vector2.zero;
        isKnockedback = false;
        canMove = true;
    }

    void TryInteract()
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var obj in nearbyObjects)
        {
            // Lógica de interacción aquí
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning)
            return;

        DoorTrigger door = other.GetComponent<DoorTrigger>();

        if (door != null)
        {
            StartCoroutine(TransitionCooldown());
            float playerOffsetFromWall = 3.0f;

            switch (door.doorDirection)
            {
                case EdgeDirection.Up:
                    transform.position += new Vector3(0, playerOffsetFromWall, 0);
                    break;
                case EdgeDirection.Down:
                    transform.position -= new Vector3(0, playerOffsetFromWall, 0);
                    break;
                case EdgeDirection.Left:
                    transform.position -= new Vector3(playerOffsetFromWall, 0, 0);
                    break;
                case EdgeDirection.Right:
                    transform.position += new Vector3(playerOffsetFromWall, 0, 0);
                    break;
            }
        }
    }

    private IEnumerator TransitionCooldown()
    {
        isTransitioning = true;
        yield return new WaitForSeconds(0.5f);
        isTransitioning = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, 1.5f);
    }
}
