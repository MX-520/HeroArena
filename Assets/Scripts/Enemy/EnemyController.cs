using UnityEngine;
using UnityEngine.AI;

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

    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private Transform attackPivot;
    [SerializeField] private bool isRunner = false;

    private float attackTimer;
    private float nextAttackTime;
    private float nextPathRefresh;
    private Vector3 lastGoal = Vector3.positiveInfinity;
    private bool retreating;
    private float retreatRetryAt;
    private float hurtDuration = 0.3f;
    private float hurtTimer;
    private bool isAttacking;

    private EnemyState currentState = EnemyState.Idle;
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Health health;
    private Collider2D enemyCollider;
    private DamageReceiver damageReceiver;
    private EnemyAttack enemyAttack;
    private NavMeshAgent agent;
    private bool hasLockedTarget;
    private RunnerGroup runnerGroup;

    public bool CanPerformHit => currentState == EnemyState.Attack && isAttacking;
    public bool UsesPersistentPursuit => isRunner || (enemyAttack != null && enemyAttack.IsRanged);
    public bool IsRunner => isRunner;
    public bool HasLockedTarget => hasLockedTarget;
    public bool IsDead => currentState == EnemyState.Dead;

    public void AssignRunnerGroup(RunnerGroup group)
    {
        runnerGroup?.Remove(this);
        runnerGroup = group;
        if (UsesPersistentPursuit && !IsDead && isActiveAndEnabled)
            runnerGroup?.Add(this);
    }

    public void LockTarget()
    {
        if (!UsesPersistentPursuit || IsDead || !isActiveAndEnabled)
            return;

        hasLockedTarget = true;
        // Preserve attack and hurt animations; Idle will resume pursuit.
        if (currentState == EnemyState.Idle)
            ChangeState(EnemyState.Chase);
    }
   

    private void Awake()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        animator = GetComponentInChildren<Animator>();
        animator.SetBool("IsRunner", isRunner);
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        enemyCollider = GetComponent<Collider2D>();
        damageReceiver = GetComponent<DamageReceiver>();
        enemyAttack = GetComponent<EnemyAttack>();
        Debug.Log("Goblin isOnNavMesh: " + agent.isOnNavMesh);
        Debug.Log("Goblin Position: " + transform.position);
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
                break;

            case EnemyState.Hurt:
                UpdateHurt();
                break;
        }
    }



    private void UpdateIdle()
    {
        // 根据实际速度判断是否播放移动动画
        bool isMoving = agent.isOnNavMesh
            && agent.velocity.sqrMagnitude > 0.01f;

        animator.SetBool("IsMoving", isMoving);

        // 停稳后清理旧路径
        if (!isMoving && agent.isOnNavMesh && agent.hasPath)
        {
            agent.ResetPath();
        }

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance <= detectionRange || (UsesPersistentPursuit && hasLockedTarget))
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

        if (enemyAttack.IsRanged && Retreat(distance)) return;

        if (distance <= enemyAttack.AttackRange && (!enemyAttack.IsRanged || enemyAttack.HasLineOfSight(player)))
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        if (!UsesPersistentPursuit && distance > detectionRange)
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

        // 当前攻击动画还没结束时，不重新判断距离和朝向
        if (isAttacking)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > enemyAttack.AttackRange || (enemyAttack.IsRanged && ((distance < 2f && Time.time >= retreatRetryAt) || !enemyAttack.HasLineOfSight(player))))
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        if (!enemyAttack.IsRanged) attackTimer -= Time.deltaTime;
        if (enemyAttack.IsRanged ? Time.time >= nextAttackTime : attackTimer <= 0)
        {
            isAttacking = true;
            FacePlayer();
            enemyAttack.BeginAttack(player);
            animator.SetBool("IsAttacking", true);

            if (isRunner)
            {
                animator.SetTrigger("AxeAttack");
            }
            else
            {
                animator.SetTrigger("Attack");
            }

            if (enemyAttack.IsRanged) nextAttackTime = Time.time + attackCooldown;
            else attackTimer = attackCooldown;
        }
    }

    private void FacePlayer()
    {
        bool left = player.position.x < transform.position.x;
        spriteRenderer.flipX = left;
        attackPivot.localScale = new Vector3(left ? -1 : 1, 1, 1);
    }

    private bool Retreat(float distance)
    {
        if (!agent.isOnNavMesh) return false;
        if (retreating && distance >= 3f) { retreating = false; lastGoal = Vector3.positiveInfinity; }
        if (!retreating && (distance >= 2f || Time.time < retreatRetryAt)) return false;
        if (!retreating || Time.time >= nextPathRefresh)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            Vector3 away = (transform.position - player.position).normalized;
            bool found = false;
            foreach (float angle in new[] { 0f, 45f, -45f, 90f, -90f })
            {
                Vector3 direction = Quaternion.Euler(0, 0, angle) * away;
                if (!NavMesh.SamplePosition(transform.position + direction * 2f, out NavMeshHit hit, 0.75f, filter)) continue;
                var path = new NavMeshPath();
                if (!NavMesh.CalculatePath(transform.position, hit.position, filter, path) || path.status != NavMeshPathStatus.PathComplete || path.corners.Length < 2) continue;
                if (Vector2.Distance(hit.position, player.position) < distance + 0.5f || Vector3.Dot((path.corners[1] - transform.position).normalized, away) < 0.1f) continue;
                agent.isStopped = false;
                agent.SetPath(path);
                found = true;
                break;
            }
            retreating = found;
            nextPathRefresh = Time.time + 0.5f;
            lastGoal = Vector3.positiveInfinity;
            if (!found) { retreatRetryAt = Time.time + 1f; return false; }
        }
        animator.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.04f);
        FacePlayer();
        return true;
    }

    private void ChasePlayer()
    {
        if (!agent.isOnNavMesh)
        {
            return;
        }
        
        if (Time.time >= nextPathRefresh)
        {
            nextPathRefresh = Time.time + 0.2f;
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            if (NavMesh.SamplePosition(player.position, out NavMeshHit target, 1.5f, filter) &&
                (!agent.hasPath || agent.isPathStale || (target.position - lastGoal).sqrMagnitude > 0.04f))
            {
                var path = new NavMeshPath();
                if (NavMesh.CalculatePath(transform.position, target.position, filter, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetPath(path);
                    lastGoal = target.position;
                }
            }
        }
        animator.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.04f);

        Vector2 direction = agent.velocity.normalized;

        if (Mathf.Abs(agent.velocity.x) > 0.15f && Mathf.Abs(direction.x) > 0.15f)
        {
            bool facingLeft = direction.x < 0;

            spriteRenderer.flipX = facingLeft;

            attackPivot.localScale = facingLeft
                ? new Vector3(-1, 1, 1)
                : new Vector3(1, 1, 1);
        }
    }



    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        if (newState == EnemyState.Attack && !enemyAttack.IsRanged) attackTimer = 0;

        if (newState == EnemyState.Chase)
        {
            if (UsesPersistentPursuit) hasLockedTarget = true;
            nextPathRefresh = 0;
            lastGoal = Vector3.positiveInfinity;

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }

        else if (newState == EnemyState.Idle)
        {
            if (agent.isOnNavMesh)
            {
                // 停止沿路径追击，但不立即强制清零速度
                agent.isStopped = true;
            }
        }
        else
        {
            // 攻击、受击、死亡仍立即停止
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }

            animator.SetBool("IsMoving", false);
        }


    }


    public void OnAttackFinished()
    {
        if (currentState != EnemyState.Attack || player == null)
            return;

        isAttacking = false;
        animator.SetBool("IsAttacking", false);

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > enemyAttack.AttackRange)
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

        isAttacking = false;
        animator.SetBool("IsAttacking", false);

        if (UsesPersistentPursuit)
        {
            LockTarget();
            runnerGroup?.Alert();
        }

        enemyAttack.CancelWindup();
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("AxeAttack");
        ChangeState(EnemyState.Hurt);

        hurtTimer = hurtDuration;

        animator.SetTrigger("Hurt");
    }

    private void OnEnable()
    {
        health.OnDamaged += OnHurt;
        health.OnDied += OnDeath;
        if (UsesPersistentPursuit && !IsDead)
            runnerGroup?.Add(this);
    }

    private void OnDisable()
    {
        health.OnDamaged -= OnHurt;
        health.OnDied -= OnDeath;
        runnerGroup?.Remove(this);
    }

    private void OnDeath()
    {
        // Health raises OnDied instead of OnDamaged for a lethal hit.
        if (UsesPersistentPursuit)
            runnerGroup?.Alert();
        runnerGroup?.Remove(this);
        enemyAttack.DisableAttack();
        ChangeState(EnemyState.Dead);
        isAttacking = false;
        animator.SetBool("IsAttacking", false);

        animator.SetBool("IsMoving", false);

        animator.SetTrigger("Death");

        enemyCollider.enabled = false;
        damageReceiver.enabled = false;

        rb.velocity = Vector2.zero;
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        Destroy(gameObject, destroyDelay);
    }
}