using Unity.Netcode;
using UnityEngine;

public class SwordHitbox : NetworkBehaviour
{
    [SerializeField] Collider hitbox;   // trigger collider on the sword
    [SerializeField] Animator animator;

    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetMouseButtonDown(0))
            AttackServerRpc();
    }

    [ServerRpc]
    void AttackServerRpc()
    {
        PlayAttackClientRpc();
    }

    [ClientRpc]
    void PlayAttackClientRpc()
    {
        animator.SetTrigger("Attack");
    }

    // Calls by animation event in swing
    public void EnableHitbox() { if (IsServer) hitbox.enabled = true; }
    public void DisableHitbox() { if (IsServer) hitbox.enabled = false; }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.TryGetComponent(out Zombie zombie))
            zombie.Die();
    }
}