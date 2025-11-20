using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class NPCController : MonoBehaviour
{
    public enum NPCState { Idle, Patrol, Wander, Talk }

    [Header("Identificación (Obligatorio para Guardado)")]
    [Tooltip("ID único para guardar el progreso de este NPC (Ej: 'Librarian_Hub')")]
    public string npcID;

    [Header("Configuración General")]
    public bool isStaticObject = false;
    [SerializeField] private NPCState defaultState = NPCState.Patrol;
    private NPCState currentState;

    [Header("Componentes")]
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movimiento")]
    public float speed = 2f;
    public float pauseDuration = 1.5f;
    private bool isPaused;
    private Vector2 currentTarget;

    [Header("Patrol Settings")]
    public Vector2[] patrolPoints;
    private int currentPatrolIndex = -1;

    [Header("Wander Settings")]
    public float wanderWidth = 5f;
    public float wanderHeight = 5f;
    private Vector2 wanderOrigin;

    [Header("Talk Settings")]
    public Animator interactPromptAnimator;
    public List<DialogueSO> primaryConversations;
    public DialogueSO defaultConversation;

    private bool playerInRange = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        wanderOrigin = transform.position;

        if (isStaticObject)
        {
            defaultState = NPCState.Idle;
            if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        }
    }

    void Start()
    {
        SwitchState(defaultState);
    }

    void Update()
    {
        if (isStaticObject)
        {
            if (playerInRange && GameInput.Instance.GetInteractPressed() && !DialogueManager.instance.isDialogueActive)
            {
                FindAndStartConversation();
            }
            return;
        }

        if (UIManager.isGamePaused) return;

        if (isPaused || currentState == NPCState.Idle || currentState == NPCState.Talk)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
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
        if (currentState == newState && Application.isPlaying) return;

        currentState = newState;
        StopAllCoroutines();
        isPaused = false;

        switch (currentState)
        {
            case NPCState.Patrol:
                if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
                StartCoroutine(PauseAndSetNextPatrolPoint());
                break;
            case NPCState.Wander:
                if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
                StartCoroutine(PauseAndSetNextWanderPoint());
                break;
            case NPCState.Talk:
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
                if (anim != null) anim.Play("Idle");
                if (interactPromptAnimator != null) interactPromptAnimator.Play("open");
                break;
            case NPCState.Idle:
                if (rb != null) rb.linearVelocity = Vector2.zero;
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
        if (playerInRange && GameInput.Instance.GetInteractPressed() && !DialogueManager.instance.isDialogueActive)
        {
            FindAndStartConversation();
        }
    }

    private void MoveTowards(Vector2 target)
    {
        if (rb == null) return;
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
        if (string.IsNullOrEmpty(npcID))
        {
            Debug.LogWarning($"El NPC {gameObject.name} no tiene NPC ID asignado. El progreso no se guardará.");
        }

        DialogueSO conversationToStart = null;

        int currentIndex = ProgressionManager.instance.GetNPCConversationIndex(npcID);

        if (currentIndex < primaryConversations.Count)
        {
            conversationToStart = primaryConversations[currentIndex];

            ProgressionManager.instance.SetNPCConversationIndex(npcID, currentIndex + 1);

            if (GameManager.Instance != null)
            {
                SaveSystem.SaveGame(
                    GameManager.Instance.codexManager,
                    GameManager.Instance.statsManager,
                    GameManager.Instance.inventoryManager,
                    GameManager.Instance.expManager
                );
            }
        }
        else if (defaultConversation != null)
        {
            conversationToStart = defaultConversation;
        }

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
            if (!isStaticObject) SwitchState(NPCState.Talk);
            else if (interactPromptAnimator != null) interactPromptAnimator.Play("open");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (gameObject.activeInHierarchy)
            {
                if (interactPromptAnimator != null) interactPromptAnimator.Play("close");
                if (!isStaticObject) SwitchState(defaultState);
            }
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