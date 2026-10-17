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

    public float DamageMultiplier { get; set; } = 1f;
    public float RangeMultiplier { get; set; } = 1f;
    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }


    public void OnAttack(InputValue value)
    {
        if (Time.timeScale > 0 && value.isPressed &&
            !playerMovement.IsHurt &&
            !playerMovement.IsRolling &&
            !playerMovement.IsDead &&
            !isAttacking)
        {
            isAttacking = true;
            GameAudio.Play(GameSound.PlayerSwing,transform.position);
            animator.SetTrigger("Attack");
        }
    }
    public void DetectHit()
    {
        if (!isAttacking || playerMovement.IsDead || playerMovement.IsHurt) return;
        Collider2D[] hitObjects =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRange * RangeMultiplier,
                targetLayer
    );


        bool playedImpact = false;
        foreach (Collider2D hit in hitObjects)
        {
            Debug.Log("Hit: " + hit.name);

            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable != null)
            {
                Health targetHealth = hit.GetComponent<Health>();
                int beforeHP = targetHealth != null ? targetHealth.CurrentHP : 0;
                damageable.TakeDamage(Mathf.RoundToInt(10 * DamageMultiplier));
                if (!playedImpact && targetHealth != null && targetHealth.CurrentHP < beforeHP)
                {
                    GameAudio.Play(GameSound.MeleeImpact, hit.transform.position);
                    playedImpact = true;
                }
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
