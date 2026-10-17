using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private bool isRanged;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private LineRenderer aimLine;
    private readonly RaycastHit2D[] obstacleHits = new RaycastHit2D[16];
    private Collider2D bodyCollider;
    private Vector2 aim;
    private bool aiming;
    private bool canAttack = true;
    public float AttackRange => attackRange;
    public bool IsRanged => isRanged;
    public int ShotsFired { get; private set; }

    private void Awake() => bodyCollider = GetComponent<Collider2D>();

    public bool HasClearAttackPath(Transform target)
    {
        if (target == null) return false;
        var targetBody = target.GetComponent<Collider2D>();
        Vector2 start = isRanged ? attackPoint.position : bodyCollider.bounds.center;
        Vector2 end = isRanged || targetBody == null ? target.position : targetBody.bounds.center;
        return IsPathClear(start, end);
    }

    public bool CanAttackTarget(Transform target) => CanAttackFromPosition(target, transform.position);

    public bool CanAttackFromPosition(Transform target, Vector3 position)
    {
        if (target == null || Vector2.Distance(position, target.position) > attackRange) return false;
        var targetBody = target.GetComponent<Collider2D>();
        Vector2 center = attackPoint.position + position - transform.position;
        float horizontalOffset = Mathf.Abs(attackPoint.position.x - transform.position.x);
        center.x = position.x + (target.position.x < position.x ? -horizontalOffset : horizontalOffset);
        if (isRanged) return IsPathClear(center, target.position);
        if (targetBody == null || !targetBody.enabled) return false;
        Vector2 feet = bodyCollider.bounds.center + position - transform.position;
        return IsPathClear(feet, targetBody.bounds.center) && Vector2.Distance(center, targetBody.ClosestPoint(center)) <= attackRange;
    }

    private bool IsPathClear(Vector2 start, Vector2 end)
    {
        var filter = new ContactFilter2D {
            useLayerMask = true,
            layerMask = LayerMask.GetMask("Default", "Destructible"),
            useTriggers = false
        };
        return Physics2D.Linecast(start, end, filter, obstacleHits) == 0;
    }

    public void BeginAttack(Transform target)
    {
        if (!isRanged || target == null) return;
        aim = target.position;
        aiming = true;
        if (aimLine != null)
        {
            aimLine.enabled = true;
            aimLine.SetPosition(0, attackPoint.position);
            aimLine.SetPosition(1, new Vector3(aim.x, aim.y, attackPoint.position.z));
        }
    }

    public void DetectHit()
    {
        if (!canAttack || GetComponent<Health>().IsDead) return;
        if (isRanged)
        {
            if (!aiming || projectilePrefab == null) return;
            if (!IsPathClear(attackPoint.position, aim)) { CancelWindup(); return; }
            Vector2 heading = aim - (Vector2)attackPoint.position;
            if (heading.sqrMagnitude > 0.001f)
            {
                Projectile shot = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
                shot.Initialize(transform, heading, damage, 6.5f, targetLayer, LayerMask.GetMask("Default", "Destructible"));
                ShotsFired++;
                GameAudio.Play(GameSound.MagicCast,transform.position);
            }
            CancelWindup();
            return;
        }
        GameAudio.Play(GameSound.EnemySwing,transform.position);
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(attackPoint.position, attackRange, targetLayer))
        {
            var receiver = hit.GetComponentInParent<DamageReceiver>();
            if (receiver != null && receiver.isActiveAndEnabled && HasClearAttackPath(receiver.transform)) receiver.TakeDamage(damage);
        }
    }

    public void CancelWindup()
    {
        aiming = false;
        if (aimLine != null) aimLine.enabled = false;
    }
    public void DisableAttack() { canAttack = false; CancelWindup(); }
    private void OnDisable() => CancelWindup();
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null) Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
