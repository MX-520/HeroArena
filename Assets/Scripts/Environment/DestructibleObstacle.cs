using UnityEngine;

public class DestructibleObstacle : MonoBehaviour
{
    private Health health;
    private DamageReceiver damageReceiver;
    private Collider2D obstacleCollider;
    private NavigationUpdater navigationUpdater;

    private void Awake()
    {
        health = GetComponent<Health>();
        damageReceiver = GetComponent<DamageReceiver>();
        obstacleCollider = GetComponent<Collider2D>();
        navigationUpdater = FindObjectOfType<NavigationUpdater>();
    }

    private void OnEnable()
    {
        health.OnDied += OnDestroyed;
    }

    private void OnDisable()
    {
        health.OnDied -= OnDestroyed;
    }

    private void OnDestroyed()
    {
        obstacleCollider.enabled = false;
        damageReceiver.enabled = false;

        Destroy(gameObject);

        if (navigationUpdater != null)
        {
            navigationUpdater.RebuildNavMesh();
        }
    }
}