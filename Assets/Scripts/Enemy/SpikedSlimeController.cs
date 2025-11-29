using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpikedSlimeController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;

    [Header("Stats")]
    public int maxHealth = 50;
    public int currentHealth;
    public int damage = 15;
    public int expReward = 10;

    [Header("Movement")]
    public float speed = 2.5f;
    public float playerDetectRange = 6f;
    public float attackRange = 1.8f;
    public Transform detectionPoint;
    public LayerMask playerLayer;
    private int facingDirection = -1;

    [Header("Combat")]
    public Transform attackPoint;
    public float weaponRange = 2f;
    public float attackCooldown = 2.5f;
    private float attackCooldownTimer;
    public float knockbackForce = 6f;
    public float knockbackTime = 0.2f;
    public float stunTime = 0.4f;

    [Header("Loot")]
    public List<LootTableEntry> lootTable;

    private enum SlimeState { Idle, Chasing, Attacking, Knockback, Dead }
    private SlimeState currentState;

    public delegate void MonsterDefeated(int exp);
    public static event MonsterDefeated OnMonsterDefeated;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        ChangeState(SlimeState.Idle);
    }

    void Update()
    {
        if (currentState == SlimeState.Dead || currentState == SlimeState.Knockback) return;

        if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;

        if (currentState == SlimeState.Attacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        CheckForPlayer();

        if (currentState == SlimeState.Chasing)
        {
            Chase();
        }
    }

    void CheckForPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectRange, playerLayer);

        if (hits.Length > 0)
        {
            player = hits[0].transform;

            LookAtPlayer();

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0)
            {
                StartCoroutine(PerformAttackSequence());
            }
            else if (distanceToPlayer > attackRange)
            {
                ChangeState(SlimeState.Chasing);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            ChangeState(SlimeState.Idle);
        }
    }

    void LookAtPlayer()
    {
        if (player == null) return;
        if ((player.position.x > transform.position.x && facingDirection == -1) ||
            (player.position.x < transform.position.x && facingDirection == 1))
        {
            Flip();
        }
    }

    IEnumerator PerformAttackSequence()
    {
        ChangeState(SlimeState.Attacking);

        int attackIndex = Random.Range(1, 4);
        string triggerName = "Attack";
        if (attackIndex == 2) triggerName = "Attack 2";
        if (attackIndex == 3) triggerName = "Attack 3";

        anim.SetTrigger(triggerName);

        yield return new WaitForSeconds(0.4f);
        CheckHit();
        yield return new WaitForSeconds(0.5f);

        attackCooldownTimer = attackCooldown;
        ChangeState(SlimeState.Idle);
    }

    void CheckHit()
    {
        if (attackPoint == null) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, playerLayer);
        if (hits.Length > 0)
        {
            PlayerController playerController = hits[0].GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                playerController.Knockback(transform, knockbackForce, stunTime);
                if (AudioManager.Instance != null && AudioManager.Instance.playerHurtSound != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.playerHurtSound);
            }
        }
    }

    void Chase()
    {
        if (player == null) return;
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;
        currentHealth -= amount;

        anim.SetTrigger("Hit");

        if (AudioManager.Instance != null && AudioManager.Instance.playerHurtSound != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.playerHurtSound);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        ChangeState(SlimeState.Dead);
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        anim.SetTrigger("Death");

        if (AudioManager.Instance != null && AudioManager.Instance.playerDeathSound != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.playerDeathSound);

        BossUnit bossUnit = GetComponent<BossUnit>();
        if (bossUnit != null)
        {
            bossUnit.SendMessage("HandleDeath");
        }

        OnMonsterDefeated?.Invoke(expReward);
        DropLoot();
        Destroy(gameObject, 2f);
    }

    private void DropLoot()
    {
        foreach (var entry in lootTable)
        {
            if (Random.value <= entry.dropChance)
            {
                if (entry.itemToDrop.isGold)
                    StatsManager.instance.AddGold(entry.itemToDrop.goldAmount);
                else
                    LootSpawner.Instance.SpawnLoot(entry.itemToDrop, 1, transform.position);
            }
        }
    }

    public void Knockback(Transform forceTransform, float force, float kbTime, float stTime)
    {
        if (currentState == SlimeState.Dead) return;
        StartCoroutine(KnockbackRoutine(forceTransform, force, kbTime, stTime));
    }

    IEnumerator KnockbackRoutine(Transform forceTransform, float force, float kbTime, float stTime)
    {
        SlimeState previousState = currentState;
        ChangeState(SlimeState.Knockback);

        Vector2 direction = (transform.position - forceTransform.position).normalized;
        rb.linearVelocity = direction * force;

        yield return new WaitForSeconds(kbTime);
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(stTime);

        if (currentHealth > 0) ChangeState(SlimeState.Idle);
    }

    private void ChangeState(SlimeState newState)
    {
        currentState = newState;

        if (currentState == SlimeState.Chasing)
        {
            anim.SetBool("Run", true);
        }
        else
        {
            anim.SetBool("Run", false);
        }
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