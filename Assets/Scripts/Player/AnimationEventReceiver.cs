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
}