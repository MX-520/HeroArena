using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int maxHP = 100;
    private int currentHP;
    public event Action OnDamaged;
    public event Action OnDied;
    private bool isDead;

    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        currentHP = maxHP;
    }


    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHP -= damage;

        Debug.Log(gameObject.name + " HP:" + currentHP);

        if (currentHP <= 0)
        {
            currentHP = 0;
            OnHealthChanged?.Invoke(currentHP, maxHP);

            Die();
            return;
        }

        OnHealthChanged?.Invoke(currentHP, maxHP);
        OnDamaged?.Invoke();
    }


    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log(gameObject.name + " Dead");
        OnDied?.Invoke();
    }

    public void RestoreFull()
    {
        currentHP = maxHP;
        isDead = false;

        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
}