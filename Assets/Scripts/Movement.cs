using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : NetworkBehaviour
{
    public float dashSpeed = 20f;  
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.1f;
    public Collider2D playerCollider;
    public GameObject playerCamera;

    private float moveSpeed = 10f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private Vector2 facing = Vector2.right;
    private const float inputEpsilon = 0.01f;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = IsOwner;
        if (playerCamera != null) playerCamera.SetActive(IsOwner);
    }

    
    void Update()
    {
        if(!IsOwner) return;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        moveInput = context.ReadValue<Vector2>();
        if (moveInput.sqrMagnitude > inputEpsilon * inputEpsilon)
        {
            animator.SetBool("isRunning", true);
            facing = moveInput.normalized;
        }
        else if (context.canceled)
        {
            animator.SetBool("isRunning", false);
            animator.SetFloat("lastInputX", facing.x);
            animator.SetFloat("lastInputY", facing.y);
        }

        animator.SetFloat("inputX", moveInput.x);
        animator.SetFloat("inputY", moveInput.y);
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (context.performed)
        {
            Vector2 attackDir = (moveInput.sqrMagnitude > inputEpsilon * inputEpsilon) ? moveInput.normalized : facing;
            animator.SetFloat("lastInputX", attackDir.x);
            animator.SetFloat("lastInputY", attackDir.y);
            animator.SetTrigger("attack");
        }
    }
}
