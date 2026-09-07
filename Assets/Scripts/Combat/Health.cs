using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int maxHP = 100;
    private int currentHP;
    public event Action OnDamaged;


    private void Awake()
    {
        currentHP = maxHP;
    }


    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        OnDamaged?.Invoke();

        Debug.Log(gameObject.name + " HP:" + currentHP);

        OnHealthChanged?.Invoke(currentHP, maxHP);


        if (currentHP <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        Debug.Log(gameObject.name + " Dead");
    }

    public event Action<int, int> OnHealthChanged;
}