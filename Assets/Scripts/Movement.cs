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
        if (playerInput != null) playerInput.enabled = IsOwner;// Enable PlayerInput only for the owner
        if (playerCamera != null) playerCamera.SetActive(IsOwner);// Activate the camera only for the owner
    }

    
    void Update()
    {
        if(!IsOwner) return;
        rb.linearVelocity = moveInput * moveSpeed;

        if (ChatManager.IsTyping)
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isRunning", false);
            animator.SetFloat("inputX", 0f);
            animator.SetFloat("inputY", 0f);
            animator.SetFloat("lastInputX", facing.x);
            animator.SetFloat("lastInputY", facing.y);
            return;
        }

    }

    public void Move(InputAction.CallbackContext context)// Move Method to handle movement input
    {
        if(!IsOwner) return;
        if(ChatManager.IsTyping) return;
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

    public void Attack(InputAction.CallbackContext context) // Attack Method to handle attack input
    {
        if (!IsOwner) return;
        if (ChatManager.IsTyping) return;
        {
            
        }
        if (context.performed)
        {
            Vector2 attackDir = (moveInput.sqrMagnitude > inputEpsilon * inputEpsilon) ? moveInput.normalized : facing;
            animator.SetFloat("lastInputX", attackDir.x);
            animator.SetFloat("lastInputY", attackDir.y);
            animator.SetTrigger("attack");
        }
    }
}
