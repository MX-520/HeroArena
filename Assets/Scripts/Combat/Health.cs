using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int maxHP = 100;
    private int currentHP;
    public event Action OnDamaged;
    public event Action OnDied;


    private void Awake()
    {
        currentHP = maxHP;
    }


    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        Debug.Log(gameObject.name + " HP:" + currentHP);

        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
            return;
        }

        OnDamaged?.Invoke();
    }


    private void Die()
    {
        Debug.Log(gameObject.name + " Dead");

        OnDied?.Invoke();
    }

    public event Action<int, int> OnHealthChanged;
}