using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private Health health;
    private Animator animator;
    private PlayerMovement playerMovement;
    private DamageReceiver damageReceiver;
    private PlayerAttack playerAttack;

    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        health = GetComponent<Health>();
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        damageReceiver = GetComponent<DamageReceiver>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void OnEnable()
    {
        health.OnDamaged += OnHurt;
        health.OnDied += OnDeath;
    }

    private void OnDisable()
    {
        health.OnDamaged -= OnHurt;
        health.OnDied -= OnDeath;
    }

    private void OnHurt()
    {
        playerAttack.CancelAttack();
        playerMovement.CancelRoll();
        playerMovement.EnterHurt();

        animator.SetTrigger("Hurt");
    }
    private void OnDeath()
    {
        playerMovement.Die();
        damageReceiver.enabled = false;

        animator.SetTrigger("Death");

        gameManager.RespawnPlayer(this);
    }
    public void Respawn(Vector3 position)
    {
        transform.position = position;

        health.RestoreFull();

        damageReceiver.enabled = true;

        playerMovement.Respawn();

        animator.Play("Idle");
    }
}