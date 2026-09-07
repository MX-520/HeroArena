using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private int damage = 10;


    public void DetectHit()
    {
        Debug.Log("Enemy Attack Hit"); 
        Collider2D[] hitObjects =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRange
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
}