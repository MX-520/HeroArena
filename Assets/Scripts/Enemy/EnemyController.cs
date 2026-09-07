using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Hurt,
        Dead
    }

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 1.5f;

    private float attackTimer;
    private float hurtDuration = 0.3f;
    private float hurtTimer;

    private EnemyState currentState = EnemyState.Idle;
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Health health;

    private void Awake()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();

        health = GetComponent<Health>();
    }

    private void Update()
    {
        if (player == null)
            return;

        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;

            case EnemyState.Chase:
                UpdateChase();
                break;

            case EnemyState.Attack:
                UpdateAttack();
                break;

            case EnemyState.Dead:
                UpdateDead();
                break;

            case EnemyState.Hurt:
                UpdateHurt();
                break;
        }
    }


    private void UpdateIdle()
    {
        animator.SetBool("IsMoving", false);

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    private void UpdateChase()
    {
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        if (distance > detectionRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        animator.SetBool("IsMoving", true);

        ChasePlayer();
    }

    private void UpdateAttack()
    {
        animator.SetBool("IsMoving", false);


        attackTimer -= Time.deltaTime;


        if (attackTimer <= 0)
        {
            animator.SetTrigger("Attack");

            attackTimer = attackCooldown;
        }
    }

    private void UpdateDead()
    {
        // 下一阶段实现
    }

    private void ChasePlayer()
    {
        Vector2 direction =
            (player.position - transform.position).normalized;


        rb.MovePosition(
            rb.position +
            direction * moveSpeed * Time.deltaTime
        );


        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        if (newState == EnemyState.Attack)
        {
            attackTimer = 0;
        }
    }

    public void OnAttackFinished()
    {
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );


        if (distance > attackRange)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    private void UpdateHurt()
    {
        animator.SetBool("IsMoving", false);

        hurtTimer -= Time.deltaTime;

        if (hurtTimer <= 0)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    public void OnHurt()
    {
        if (currentState == EnemyState.Dead)
            return;


        ChangeState(EnemyState.Hurt);

        hurtTimer = hurtDuration;

        animator.SetTrigger("Hurt");
    }

    private void OnEnable()
    {
        health.OnDamaged += OnHurt;
    }

    private void OnDisable()
    {
        health.OnDamaged -= OnHurt;
    }
}