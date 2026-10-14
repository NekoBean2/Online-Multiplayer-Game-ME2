using Unity.Netcode;
using UnityEngine;

public class Zombie : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float killRange = 0.7f;
    [SerializeField] float despawnDelay = 0.1f;
    [SerializeField] Animator animator;          // optional
    [SerializeField] Collider2D bodyCollider;

    readonly NetworkVariable<bool> isDead = new();

    public override void OnNetworkSpawn()
    {
        isDead.OnValueChanged += OnDeadChanged;
        if (isDead.Value) OnDeadChanged(false, true);

    }

    public override void OnNetworkDespawn() => isDead.OnValueChanged -= OnDeadChanged;

    void Update()
    {
        if (!IsServer || isDead.Value) return;

        PlayerHealth nearest = FindNearestPlayer();
        if (nearest == null) return;

        Vector2 current = transform.position;
        Vector2 target = nearest.transform.position;

        Vector2 next = Vector2.MoveTowards(current, target, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(next.x, next.y, transform.position.z);

        if (Vector2.Distance(current, target) <= killRange)
            nearest.Kill();
    }

    PlayerHealth FindNearestPlayer()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null || !nm.IsListening) return null;

        PlayerHealth best = null;
        float bestDist = float.MaxValue;

        foreach (var client in nm.ConnectedClientsList)
        {
            if (client == null || client.PlayerObject == null) continue;
            if (!client.PlayerObject.TryGetComponent(out PlayerHealth p)) continue;

            float d = ((Vector2)p.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = p;
            }
        }
        return best;
    }

    public void Die()
    {
        if (!IsServer || isDead.Value) return;
        isDead.Value = true;
        FindAnyObjectByType<ZombieSpawner>()?.OnZombieDied();
        Invoke(nameof(DespawnSelf), despawnDelay);
    }

    void DespawnSelf() => NetworkObject.Despawn();

    void OnDeadChanged(bool _, bool dead)
    {
        if (!dead) return;
        if (bodyCollider != null) bodyCollider.enabled = false;
        if (animator != null) animator.SetTrigger("Die");
    }
}