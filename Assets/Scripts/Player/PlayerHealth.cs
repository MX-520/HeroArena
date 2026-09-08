using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private Health health;
    private Animator animator;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        health = GetComponent<Health>();
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        health.OnDamaged += OnHurt;
    }

    private void OnDisable()
    {
        health.OnDamaged -= OnHurt;
    }

    private void OnHurt()
    {
        playerMovement.EnterHurt();
        animator.SetTrigger("Hurt");
    }
}