using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class NPCController : MonoBehaviour
{
    public enum NPCState { Idle, Patrol, Wander, Talk }
    [Tooltip("El comportamiento que tendrá el NPC cuando el jugador no esté cerca.")]
    [SerializeField] private NPCState defaultState = NPCState.Patrol;
    private NPCState currentState;

    [Header("Componentes")]
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movimiento")]
    public float speed = 2f;
    [Tooltip("Tiempo que el NPC se detiene antes de elegir un nuevo destino.")]
    public float pauseDuration = 1.5f;
    private bool isPaused;
    private Vector2 currentTarget;

    [Header("Patrol Settings")]
    [Tooltip("Una lista de puntos que el NPC seguirá en orden.")]
    public Vector2[] patrolPoints;
    private int currentPatrolIndex = -1;

    [Header("Wander Settings")]
    [Tooltip("El ancho y alto del área donde el NPC puede deambular.")]
    public float wanderWidth = 5f;
    public float wanderHeight = 5f;
    private Vector2 wanderOrigin;

    [Header("Talk Settings")]
    [Tooltip("El animador para el ícono de interacción (opcional).")]
    public Animator interactPromptAnimator;
    [Tooltip("Lista de conversaciones que este NPC puede tener.")]
    public List<DialogueSO> conversations;
    private bool playerInRange = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        wanderOrigin = transform.position;
    }

    void Start()
    {
        SwitchState(defaultState);
    }

    void Update()
    {
        if (isPaused || currentState == NPCState.Idle || currentState == NPCState.Talk)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            switch (currentState)
            {
                case NPCState.Patrol:
                    HandlePatrol();
                    break;
                case NPCState.Wander:
                    HandleWander();
                    break;
            }
        }

        if (currentState == NPCState.Talk)
        {
            HandleTalkInteraction();
        }
    }

    private void SwitchState(NPCState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        StopAllCoroutines();
        isPaused = false;

        switch (currentState)
        {
            case NPCState.Patrol:
                rb.bodyType = RigidbodyType2D.Dynamic;
                StartCoroutine(PauseAndSetNextPatrolPoint());
                break;
            case NPCState.Wander:
                rb.bodyType = RigidbodyType2D.Dynamic;
                StartCoroutine(PauseAndSetNextWanderPoint());
                break;
            case NPCState.Talk:
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
                if (anim != null) anim.Play("Idle");
                if (interactPromptAnimator != null) interactPromptAnimator.Play("open");
                break;
            case NPCState.Idle:
                rb.linearVelocity = Vector2.zero;
                if (anim != null) anim.Play("Idle");
                break;
        }
    }

    private void HandlePatrol()
    {
        if (patrolPoints.Length == 0) return;

        if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
        {
            StartCoroutine(PauseAndSetNextPatrolPoint());
        }
        else
        {
            MoveTowards(currentTarget);
        }
    }


    private void HandleWander()
    {
        if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
        {
            StartCoroutine(PauseAndSetNextWanderPoint());
        }
        else
        {
            MoveTowards(currentTarget);
        }
    }

    private void HandleTalkInteraction()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            FindAndStartConversation();
        }
    }

    private void MoveTowards(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        FlipSprite(direction.x);
        rb.linearVelocity = direction * speed;
    }

    private void FlipSprite(float directionX)
    {
        if (directionX < 0 && transform.localScale.x > 0 || directionX > 0 && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
    }

    private IEnumerator PauseAndSetNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) yield break;

        isPaused = true;
        if (anim != null) anim.Play("Idle");
        yield return new WaitForSeconds(pauseDuration);

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        currentTarget = patrolPoints[currentPatrolIndex];

        isPaused = false;
        if (anim != null) anim.Play("Walk");
    }

    private IEnumerator PauseAndSetNextWanderPoint()
    {
        isPaused = true;
        if (anim != null) anim.Play("Idle");
        yield return new WaitForSeconds(pauseDuration);

        float randomX = Random.Range(wanderOrigin.x - wanderWidth / 2f, wanderOrigin.x + wanderWidth / 2f);
        float randomY = Random.Range(wanderOrigin.y - wanderHeight / 2f, wanderOrigin.y + wanderHeight / 2f);
        currentTarget = new Vector2(randomX, randomY);

        isPaused = false;
        if (anim != null) anim.Play("Walk");
    }

    private void FindAndStartConversation()
    {
        if (conversations.Count == 0) return;
        if (DialogueManager.instance.isDialogueActive) return;

        DialogueSO conversationToStart = conversations[0];

        if (conversationToStart != null)
        {
            DialogueManager.instance.StartDialogue(conversationToStart);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            SwitchState(NPCState.Talk);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPromptAnimator != null) interactPromptAnimator.Play("close");
            SwitchState(defaultState);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (defaultState == NPCState.Wander)
        {
            Gizmos.color = Color.cyan;
            Vector2 origin = Application.isPlaying ? wanderOrigin : (Vector2)transform.position;
            Gizmos.DrawWireCube(origin, new Vector3(wanderWidth, wanderHeight, 0));
        }

        if (defaultState == NPCState.Patrol && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Gizmos.DrawSphere(patrolPoints[i], 0.2f);
                if (i > 0)
                {
                    Gizmos.DrawLine(patrolPoints[i - 1], patrolPoints[i]);
                }
            }
            Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1], patrolPoints[0]);
        }
    }
}