using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private PlayerAttack playerAttack;


    private void Awake()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
    }


    public void DetectHit()
    {
        playerAttack.DetectHit();
    }
    public void OnHurtFinished()
    {
        PlayerMovement playerMovement =
            GetComponentInParent<PlayerMovement>();

        playerMovement.ExitHurt();
    }
    public void OnAttackFinished()
    {
        PlayerAttack playerAttack =
            GetComponentInParent<PlayerAttack>();

        playerAttack.OnAttackFinished();
    }
}