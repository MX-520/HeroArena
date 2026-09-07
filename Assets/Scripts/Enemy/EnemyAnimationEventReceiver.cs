using UnityEngine;

public class EnemyAnimationEventReceiver : MonoBehaviour
{
    private EnemyAttack enemyAttack;
    private EnemyController enemyController;


    private void Awake()
    {
        enemyAttack = GetComponentInParent<EnemyAttack>();
        enemyController = GetComponentInParent<EnemyController>();
    }


    public void DetectHit()
    {
        enemyAttack.DetectHit();
    }


    public void OnAttackFinished()
    {
        enemyController.OnAttackFinished();
    }
}