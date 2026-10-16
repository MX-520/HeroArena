using UnityEngine;

// Swept 2D casts prevent fast projectiles tunnelling through thin colliders.
public class Projectile : MonoBehaviour
{
    private readonly RaycastHit2D[] hits = new RaycastHit2D[16];
    private Transform owner;
    private Vector2 direction;
    private float speed;
    private float remaining;
    private int damage;
    private int targetMask;
    private int blockingMask;
    private bool initialized;
    public int Damage => damage;
    public Transform Owner => owner;

    public void Initialize(Transform source, Vector2 heading, int amount, float velocity, int targets, int blockers, float lifetime = 3f)
    {
        owner = source;
        direction = heading.normalized;
        damage = amount;
        speed = velocity;
        targetMask = targets;
        blockingMask = blockers;
        remaining = lifetime;
        initialized = true;
        transform.right = direction;
    }

    private void Update()
    {
        if (!initialized || Time.deltaTime <= 0) return;
        float distance = speed * Time.deltaTime;
        var filter = new ContactFilter2D { useLayerMask = true, layerMask = targetMask | blockingMask | LayerMask.GetMask("ProjectileHitbox"), useTriggers = true };
        int count = Physics2D.CircleCast(transform.position, 0.08f, direction, filter, hits, distance);
        for (int i = 0; i < count; i++)
        {
            Collider2D collider = hits[i].collider;
            if (owner != null && collider.transform.IsChildOf(owner)) continue;
            bool bodyHitbox = collider.gameObject.layer == LayerMask.NameToLayer("ProjectileHitbox");
            if (collider.isTrigger && !bodyHitbox) continue;
            var receiver = collider.GetComponentInParent<DamageReceiver>();
            int recipientLayer = receiver == null ? collider.gameObject.layer : receiver.gameObject.layer;
            if (bodyHitbox && ((targetMask & (1 << recipientLayer)) == 0 || receiver == null || !receiver.isActiveAndEnabled)) continue;
            if ((targetMask & (1 << recipientLayer)) != 0 && receiver != null && receiver.isActiveAndEnabled)
                receiver.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        transform.position += (Vector3)(direction * distance);
        remaining -= Time.deltaTime;
        if (remaining <= 0) Destroy(gameObject);
    }
}
