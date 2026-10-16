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
    private Vector2 aim;
    private bool aiming;
    private bool canAttack = true;
    public float AttackRange => attackRange;
    public bool IsRanged => isRanged;
    public int ShotsFired { get; private set; }

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
            Vector2 heading = aim - (Vector2)attackPoint.position;
            if (heading.sqrMagnitude > 0.001f)
            {
                Projectile shot = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
                shot.Initialize(transform, heading, damage, 6.5f, targetLayer, LayerMask.GetMask("Default", "Destructible"));
                ShotsFired++;
            }
            CancelWindup();
            return;
        }
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(attackPoint.position, attackRange, targetLayer))
        {
            var receiver = hit.GetComponentInParent<DamageReceiver>();
            if (receiver != null && receiver.isActiveAndEnabled) receiver.TakeDamage(damage);
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
