using UnityEngine;

public class HealthDebug : MonoBehaviour
{
    private Health health;


    private void Awake()
    {
        health = GetComponent<Health>();
    }


    private void OnEnable()
    {
        health.OnHealthChanged += OnHealthChanged;
    }


    private void OnDisable()
    {
        health.OnHealthChanged -= OnHealthChanged;
    }


    private void OnHealthChanged(int currentHP, int maxHP)
    {
        Debug.Log(
            "收到HP变化通知: "
            + currentHP
            + "/"
            + maxHP
        );
    }
}