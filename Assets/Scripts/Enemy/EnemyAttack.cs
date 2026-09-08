using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask targetLayer;

    private bool canAttack = true;


    public void DetectHit()
    {
        if (!canAttack)
            return;

        Collider2D[] hitObjects =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRange,
                targetLayer
    );

        foreach (Collider2D hit in hitObjects)
        {
            DamageReceiver receiver =
                hit.GetComponent<DamageReceiver>();

            if (receiver != null)
            {
                receiver.TakeDamage(damage);
            }
        }
    }

    public void DisableAttack()
    {
        canAttack = false;
    }


}