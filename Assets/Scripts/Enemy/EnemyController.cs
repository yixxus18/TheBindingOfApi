using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;

    [Header("Stats")]
    public int maxHealth = 30;
    public int currentHealth;
    public int damage = 10;
    public int expReward = 3;

    [Header("Movement")]
    public float speed = 3f;
    public float playerDetectRange = 5f;
    public float attackRange = 1.5f;
    public Transform detectionPoint;
    public LayerMask playerLayer;
    private int facingDirection = -1;

    [Header("Combat")]
    public Transform attackPoint;
    public float weaponRange = 1.5f;
    public float attackCooldown = 2f;
    private float attackCooldownTimer;
    public float knockbackForce = 5f;
    public float knockbackTime = 0.2f;
    public float stunTime = 0.3f;

    [Header("State")]
    private EnemyState enemyState;
    private bool isInKnockback = false;

    public delegate void MonsterDefeated(int exp);
    public static event MonsterDefeated OnMonsterDefeated;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        ChangeState(EnemyState.Idle);
    }

    void Update()
    {
        if (isInKnockback)
            return;

        if (enemyState != EnemyState.Knockback)
        {
            CheckForPlayer();

            if (attackCooldownTimer > 0)
                attackCooldownTimer -= Time.deltaTime;

            if (enemyState == EnemyState.Chasing)
            {
                Chase();
            }
            else if (enemyState == EnemyState.Attacking)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void CheckForPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectRange, playerLayer);

        if (hits.Length > 0)
        {
            player = hits[0].transform;
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0)
            {
                attackCooldownTimer = attackCooldown;
                ChangeState(EnemyState.Attacking);
            }
            else if (distanceToPlayer > attackRange && enemyState != EnemyState.Attacking)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            ChangeState(EnemyState.Idle);
        }
    }

    void Chase()
    {
        if (player == null)
            return;

        if (player.position.x > transform.position.x && facingDirection == -1 ||
            player.position.x < transform.position.x && facingDirection == 1)
        {
            Flip();
        }

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    public void Attack()
    {
        if (attackPoint == null)
            return;

        Debug.Log("🗡️ Enemigo atacando!");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, playerLayer);

        if (hits.Length > 0)
        {
            PlayerController playerController = hits[0].GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                playerController.Knockback(transform, knockbackForce, stunTime);
                Debug.Log("✅ Player golpeado por enemigo!");
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0)
        {
            Debug.LogWarning("⚠️ Enemigo ya está muerto, ignorando daño");
            return; // ✅ Ya está muerto, no hacer nada
        }

        currentHealth -= amount;
        Debug.Log($"Enemigo recibió {amount} de daño. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"💀 {gameObject.name} murió! Dando {expReward} XP");

        OnMonsterDefeated?.Invoke(expReward);

        Destroy(gameObject);
    }



    public void Knockback(Transform forceTransform, float force, float kbTime, float stTime)
    {
        if (isInKnockback) return;

        Debug.Log("⚡ Enemigo recibiendo knockback!");

        StartCoroutine(KnockbackCoroutine(forceTransform, force, kbTime, stTime));
    }

    IEnumerator KnockbackCoroutine(Transform forceTransform, float force, float kbTime, float stTime)
    {
        isInKnockback = true;
        ChangeState(EnemyState.Knockback);
        Vector2 direction = (transform.position - forceTransform.position).normalized;
        rb.linearVelocity = direction * force;
        yield return new WaitForSeconds(kbTime);
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(stTime);
        isInKnockback = false;
        ChangeState(EnemyState.Idle);
        Debug.Log("✅ Knockback del enemigo terminado");
    }

    public void ChangeState(EnemyState newState)
    {
        if (enemyState == EnemyState.Idle)
            anim.SetBool("isIdle", false);
        else if (enemyState == EnemyState.Chasing)
            anim.SetBool("isChasing", false);
        else if (enemyState == EnemyState.Attacking)
            anim.SetBool("isAttacking", false);

        enemyState = newState;

        if (enemyState == EnemyState.Idle)
            anim.SetBool("isIdle", true);
        else if (enemyState == EnemyState.Chasing)
            anim.SetBool("isChasing", true);
        else if (enemyState == EnemyState.Attacking)
            anim.SetBool("isAttacking", true);
    }

    private void OnDrawGizmosSelected()
    {
        if (detectionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionPoint.position, playerDetectRange);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
        }
    }
}

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking,
    Knockback
}
