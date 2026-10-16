using UnityEngine;

[RequireComponent(typeof(Health))]
public class DropOnDeath : MonoBehaviour
{
    [SerializeField] private Pickup[] prefabs;
    [SerializeField, Range(0, 1)] private float chance = 0.35f;
    private Health health;
    private bool dropped;
    public bool GuaranteeHeal { get; set; }
    private void Awake() => health = GetComponent<Health>();
    private void OnEnable() => health.OnDied += Drop;
    private void OnDisable() => health.OnDied -= Drop;
    private void Drop()
    {
        if (dropped) return;
        dropped = true;
        if (prefabs == null || prefabs.Length < 4 || (!GuaranteeHeal && Random.value > chance)) return;
        int index = GuaranteeHeal ? 0 : Random.Range(0, 4);
        Instantiate(prefabs[index], transform.position, Quaternion.identity);
    }
}
