using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : NetworkBehaviour
{
    [SerializeField] SpriteRenderer sprite;      // the sprite that gets flipped
    [SerializeField] Vector2 attackOffset = new Vector2(1f, 0f);  // offset when facing right
    [SerializeField] float attackRadius = 0.8f;
    [SerializeField] float attackCooldown = 0.6f;
    [SerializeField] float hitDelay = 0.15f;
    [SerializeField] float hitWindow = 0.25f;
    [SerializeField] Animator animator;          // optional

    float nextAttackTime;

    void Update()
    {
        if (!IsOwner) return;

        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            // How player flips
            bool facingLeft = sprite != null && sprite.flipX;
            // bool facingLeft = transform.localScale.x < 0;   // if flipped with scale instead of sprite.flipX

            AttackServerRpc(facingLeft);
        }
    }

    [ServerRpc]
    void AttackServerRpc(bool facingLeft)
    {
        PlayAttackClientRpc();
        StartCoroutine(DoHit(facingLeft));
    }

    [ClientRpc]
    void PlayAttackClientRpc()
    {
        if (animator != null) animator.SetTrigger("Attack");
    }

    Vector2 GetAttackCenter(bool facingLeft)
    {
        Vector2 offset = attackOffset;
        if (facingLeft) offset.x = -offset.x;
        return (Vector2)transform.position + offset;
    }

    IEnumerator DoHit(bool facingLeft)
    {
        yield return new WaitForSeconds(hitDelay);

        float end = Time.time + hitWindow;
        while (Time.time < end)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(GetAttackCenter(facingLeft), attackRadius);
            foreach (var h in hits)
            {
                var zombie = h.GetComponentInParent<Zombie>();
                if (zombie != null) zombie.Die();
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(false), attackRadius);   // right
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetAttackCenter(true), attackRadius);    // left
    }
}