using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement_B : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float airControlMultiplier = 0.3f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.5f;

    [Header("Physics")]
    [SerializeField] private float gravityMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("VFX and SFXs")]
    [SerializeField] private GameObject deathParticle;
    [SerializeField] private AudioClip deathSound;

    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    private Rigidbody rb;
    private PlayerControls inputActions;

    private Vector2 moveInput;
    private bool jumpPressed;

    private Vector2 receivedMove;
    private bool receivedJump;

    private float lastJumpTime = Mathf.NegativeInfinity;

    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerControls();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void OnNetworkSpawn()
    {
        // Bu obje Server'da mı?
        if (IsServer)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            isAlive.Value = true;
        }
        else
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Sadece Owner input alabilir
        if (!IsOwner)
        {
            inputActions.Disable();
            return;
        }

        inputActions.Enable();
    }


    private void OnEnable()
    {
        if (IsOwner)
        {
            inputActions.Enable();
            inputActions.Player.Movement.performed += OnMove;
            inputActions.Player.Movement.canceled += OnMove;
            inputActions.Player.Jump.performed += OnJump;
        }
    }

    private void OnDisable()
    {
        if (IsOwner)
        {
            inputActions.Disable();
            inputActions.Player.Movement.performed -= OnMove;
            inputActions.Player.Movement.canceled -= OnMove;
            inputActions.Player.Jump.performed -= OnJump;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 rawMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        bool jump = Input.GetKeyDown(KeyCode.Space);

        SendInputServerRpc(rawMove, jump);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendInputServerRpc(Vector2 move, bool jump, ServerRpcParams rpcParams = default)
    {
        receivedMove = move;
        receivedJump = jump;
    }
    private void FixedUpdate()
    {
        if (!IsServer) return;

        Debug.Log($"[{OwnerClientId}] IsServer: {IsServer}, IsKinematic: {rb.isKinematic}, UseGravity: {rb.useGravity}");


        ApplyMovement(receivedMove);
        ApplyJump(receivedJump);
        ApplyGravity();
        AnimationHandler();
        receivedJump = false;
    }

    private void ApplyMovement(Vector2 moveInput)
    {
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (inputDir.sqrMagnitude < 0.01f) return;

        float multiplier = IsGrounded() ? 1f : 1f - airControlMultiplier;
        Vector3 move = inputDir.normalized * moveSpeed * multiplier * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + move);

        Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
    }

    private void ApplyJump(bool jump)
    {
        if (jump && IsGrounded() && Time.time >= lastJumpTime + jumpCooldown)
        {
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(vel.x, jumpForce, vel.z);
            lastJumpTime = Time.time;
        }
    }

    private void ApplyGravity()
    {
        if (!IsGrounded())
        {
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        jumpPressed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("deadLine"))
        {
            DeadServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeadServerRpc()
    {
        isAlive.Value = false;
        GameManager_B.instance.RemoveAlivePlayerServerRpc(OwnerClientId);
        ShowDeathEffectClientRpc(transform.position);
        GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    private void ShowDeathEffectClientRpc(Vector3 pos)
    {
        if (deathParticle) Instantiate(deathParticle, pos, Quaternion.identity);
        if (deathSound) audioSource.PlayOneShot(deathSound);
    }

    [ServerRpc(RequireOwnership = false)]
    void SetAnimBoolServerRpc(string param, bool value)
    {
        SetAnimBoolClientRpc(param, value);
    }

    [ClientRpc]
    void SetAnimBoolClientRpc(string param, bool value)
    {
        //Debug.Log($"[ClientRpc] SetBool {param} = {value} on {OwnerClientId}");

        if (animator == null)
        {
            Debug.LogError($"Animator is NULL on client {OwnerClientId}!");
            return;
        }

        animator.SetBool(param, value);
    }


    void AnimationHandler()
    {
        Vector3 velocity = rb.linearVelocity;
        bool isGrounded = IsGrounded();
        bool isTryingToMove = receivedMove.magnitude > 0.1f;

        if (isGrounded && isTryingToMove)
        {
            animator.SetBool("isRunning", true);
            SetAnimBoolServerRpc("isRunning", true);
        }
        else if (isGrounded && !isTryingToMove && animator.GetBool("isRunning"))
        {
            animator.SetBool("isRunning", false);
            SetAnimBoolServerRpc("isRunning", false);
        }

        if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            SetAnimBoolServerRpc("isJumping", false);
            SetAnimBoolServerRpc("isFalling", false);
        }
        else if (velocity.y > 0.1f)
        {
            animator.SetBool("isJumping", true);
            SetAnimBoolServerRpc("isJumping", true);
            animator.SetBool("isFalling", false);
            SetAnimBoolServerRpc("isFalling", false);
        }
        else if (velocity.y < -0.1f)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", true);
            SetAnimBoolServerRpc("isJumping", false);
            SetAnimBoolServerRpc("isFalling", true);
        }
    }


}
