using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.8f;


    private Animator animator;


    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }


    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            animator.SetTrigger("Attack");

        }
    }
    public void DetectHit()
    {
        Collider2D[] hitObjects =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRange
            );


        foreach (Collider2D hit in hitObjects)
        {
            Debug.Log("Hit: " + hit.name);
        }
    }
}