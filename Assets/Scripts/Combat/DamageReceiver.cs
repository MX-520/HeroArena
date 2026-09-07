using UnityEngine;

public class DamageReceiver : MonoBehaviour, IDamageable
{
    private Health health;

    private bool isInvincible;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible)
            return;

        health.TakeDamage(damage);
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }
}