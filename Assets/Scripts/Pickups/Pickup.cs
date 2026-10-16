using UnityEngine;

public enum PickupKind { Heal, Barrage, Giant, Shield }

public class Pickup : MonoBehaviour
{
    [SerializeField] private PickupKind kind;
    [SerializeField] private float lifetime = 30f;
    private Vector3 origin;
    private float born;
    private bool consumed;
    public PickupKind Kind => kind;
    private void Awake() { origin = transform.position; born = Time.time; }
    private void Update()
    {
        transform.position = origin + Vector3.up * (Mathf.Sin((Time.time - born) * 3f) * 0.07f);
        if (Time.time - born >= lifetime) Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collider) => TryCollect(collider.GetComponentInParent<PlayerEffects>());
    private void OnTriggerStay2D(Collider2D collider) => TryCollect(collider.GetComponentInParent<PlayerEffects>());
    public bool TryCollect(PlayerEffects player)
    {
        if (consumed || player == null || !player.TryApply(kind)) return false;
        consumed = true;
        gameObject.SetActive(false);
        Destroy(gameObject);
        return true;
    }
}
