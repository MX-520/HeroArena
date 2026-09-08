using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private LayerMask targetLayer;


    private Animator animator;
    private PlayerMovement playerMovement;
    private bool isAttacking;

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }


    public void OnAttack(InputValue value)
    {
        if (value.isPressed &&
            !playerMovement.IsHurt &&
            !playerMovement.IsRolling &&
            !playerMovement.IsDead &&
            !isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("Attack");
        }
    }
    public void DetectHit()
    {
        Collider2D[] hitObjects =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRange,
                targetLayer
    );


        foreach (Collider2D hit in hitObjects)
        {
            Debug.Log("Hit: " + hit.name);

            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(10);
            }
        }
    }
    public void OnAttackFinished()
    {
        isAttacking = false;
    }
    public void CancelAttack()
    {
        isAttacking = false;
    }
}