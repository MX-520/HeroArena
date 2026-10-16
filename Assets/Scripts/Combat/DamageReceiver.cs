using UnityEngine;

public class DamageReceiver : MonoBehaviour, IDamageable
{
    private Health health;

    private bool isInvincible;
    private bool shielded;
    public void SetShield(bool value) => shielded = value;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public void TakeDamage(int damage)
    {
        if (!isActiveAndEnabled || isInvincible || shielded)
            return;

        health.TakeDamage(damage);
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }
}