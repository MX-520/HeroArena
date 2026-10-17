using UnityEngine;

/// <summary>Health feedback and travelled-distance footsteps; independent of sprite/Animator assets.</summary>
[RequireComponent(typeof(Health))]
public sealed class ActorAudio : MonoBehaviour
{
    public enum ActorKind { Enemy, Player, WoodObstacle, StoneObstacle }
    [SerializeField] private ActorKind kind;
    private Health health;
    private PlayerMovement movement;
    private Vector3 previous;
    private float travelled;
    private void Awake() {health=GetComponent<Health>();movement=GetComponent<PlayerMovement>();}
    private void OnEnable() {previous=transform.position;travelled=0;health.OnDamaged+=Hurt;health.OnDied+=Died;}
    private void OnDisable() {health.OnDamaged-=Hurt;health.OnDied-=Died;}
    private void Hurt()
    {
        if(kind==ActorKind.Player) GameAudio.Play(GameSound.PlayerHurt,transform.position);
        else if(kind==ActorKind.Enemy) GameAudio.Play(GameSound.EnemyHurt,transform.position);
    }
    private void Died()=>GameAudio.Play(kind==ActorKind.Player?GameSound.PlayerDeath:
        kind==ActorKind.Enemy?GameSound.EnemyDeath:kind==ActorKind.WoodObstacle?GameSound.WoodBreak:GameSound.StoneBreak,transform.position);
    private void Update()
    {
        float distance=Vector2.Distance(previous,transform.position);previous=transform.position;
        if(kind!=ActorKind.Player || movement==null || Time.deltaTime<=0 || movement.IsRolling || movement.IsHurt || movement.IsDead || distance>2)
        {travelled=0;return;}
        travelled+=distance;
        if(travelled>=.85f) {travelled%=.85f;GameAudio.Play(GameSound.Footstep,transform.position);}
    }
}
