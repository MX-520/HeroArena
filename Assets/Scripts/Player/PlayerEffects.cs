using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [SerializeField] private float barrageDuration = 10f;
    [SerializeField] private float giantDuration = 8f;
    [SerializeField] private float shieldDuration = 5f;
    [SerializeField] private Sprite shieldSprite;
    private float barrageUntil, giantUntil, shieldUntil;
    private Health health;
    private PlayerShooting shooting;
    private PlayerAttack melee;
    private PlayerMovement movement;
    private DamageReceiver receiver;
    private Transform visual;
    private Vector3 originalScale;
    private SpriteRenderer shield;
    public bool GiantActive => Remaining(PickupKind.Giant) > 0;
    public float Remaining(PickupKind kind) => Mathf.Max(0, (kind == PickupKind.Barrage ? barrageUntil : kind == PickupKind.Giant ? giantUntil : kind == PickupKind.Shield ? shieldUntil : 0) - Time.time);

    private void Awake()
    {
        health = GetComponent<Health>();
        shooting = GetComponent<PlayerShooting>();
        melee = GetComponent<PlayerAttack>();
        movement = GetComponent<PlayerMovement>();
        receiver = GetComponent<DamageReceiver>();
        visual = GetComponentInChildren<Animator>().transform;
        originalScale = visual.localScale;
        var guard = new GameObject("ShieldEffect");
        guard.transform.SetParent(transform, false);
        guard.transform.localScale = Vector3.one * 0.7f;
        shield = guard.AddComponent<SpriteRenderer>();
        shield.sprite = shieldSprite;
        shield.sortingOrder = 25;
        shield.color = new Color(0.4f, 1, 1, 0.42f);
        shield.enabled = false;
    }
    private void OnEnable() => health.OnDied += Clear;
    private void OnDisable() { health.OnDied -= Clear; Clear(); }
    public bool TryApply(PickupKind kind)
    {
        if (!isActiveAndEnabled || health.IsDead) return false;
        switch (kind)
        {
            case PickupKind.Heal: return health.Heal(25) > 0;
            case PickupKind.Barrage: barrageUntil = Time.time + barrageDuration; break;
            case PickupKind.Giant: giantUntil = Time.time + giantDuration; break;
            case PickupKind.Shield: shieldUntil = Time.time + shieldDuration; break;
        }
        Apply();
        return true;
    }
    private void Update() => Apply();
    private void Apply()
    {
        bool giant = GiantActive;
        shooting.Barrage = Remaining(PickupKind.Barrage) > 0;
        shooting.DamageMultiplier = giant ? 1.8f : 1;
        melee.DamageMultiplier = giant ? 1.8f : 1;
        melee.RangeMultiplier = giant ? 1.5f : 1;
        movement.SpeedMultiplier = giant ? 1.25f : 1;
        visual.localScale = originalScale * (giant ? 1.5f : 1);
        bool guarded = Remaining(PickupKind.Shield) > 0;
        receiver.SetShield(guarded);
        shield.enabled = guarded;
    }
    public void Clear()
    {
        barrageUntil = giantUntil = shieldUntil = 0;
        if (health != null) Apply();
    }
}
