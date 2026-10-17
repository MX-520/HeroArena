using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private int damage = 12;
    [SerializeField] private float interval = 0.32f;
    [SerializeField] private float projectileSpeed = 16f;
    private PlayerMovement movement;
    private PlayerAttack melee;
    private Camera aimCamera;
    private float nextShot;
    public float DamageMultiplier { get; set; } = 1f;
    public bool Barrage { get; set; }
    public int ShotsFired { get; private set; }

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        melee = GetComponent<PlayerAttack>();
        aimCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null || aimCamera == null) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (Mouse.current.leftButton.isPressed)
        {
            Vector3 screen = Mouse.current.position.ReadValue();
            screen.z = -aimCamera.transform.position.z + transform.position.z;
            TryShoot(aimCamera.ScreenToWorldPoint(screen));
        }
    }

    public bool TryShoot(Vector2 target)
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0 || projectilePrefab == null ||
            movement.IsDead || movement.IsHurt || movement.IsRolling || melee.IsAttacking || Time.time < nextShot)
            return false;
        Vector2 direction = target - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.001f) return false;
        nextShot = Time.time + interval;
        direction.Normalize();
        int count = Barrage ? 3 : 1;
        for (int i = 0; i < count; i++)
        {
            float angle = count == 1 ? 0 : (i - 1) * 10f;
            Vector2 heading = Quaternion.Euler(0, 0, angle) * direction;
            Projectile shot = Instantiate(projectilePrefab, transform.position + (Vector3)(heading * 0.55f), Quaternion.identity);
            shot.Initialize(transform, heading, Mathf.RoundToInt(damage * DamageMultiplier), projectileSpeed,
                LayerMask.GetMask("Enemy", "Destructible"), LayerMask.GetMask("Default"));
            ShotsFired++;
        }
        GameAudio.Play(GameSound.PlayerShot,transform.position);
        return true;
    }
}

