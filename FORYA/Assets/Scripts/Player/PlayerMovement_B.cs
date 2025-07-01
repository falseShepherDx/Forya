using System;
using System.Collections.Generic;
using System.Linq;
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

    private float lastJumpTime = Mathf.NegativeInfinity;

    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true);

    private Vector2 receivedMove;
    private bool receivedJump;

    private Vector2 lastSentMove = Vector2.zero;
    private bool lastSentJump = false;
    struct PlayerInputData
    {
        public float timestamp;
        public Vector2 move;
        public bool jump;

        public PlayerInputData(float time, Vector2 move, bool jump)
        {
            this.timestamp = time;
            this.move = move;
            this.jump = jump;
        }
    }

    private List<PlayerInputData> inputBuffer = new List<PlayerInputData>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerControls();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner && IsServer)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            isAlive.Value = true;
        }
        else if (IsOwner && !IsServer)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            if (IsOwner && FindObjectOfType<DebugGhostSystem>() != null)
                FindObjectOfType<DebugGhostSystem>().playerTransform = transform;
        }
        else
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

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
        }
    }

    private void OnDisable()
    {
        if (IsOwner)
        {
            inputActions.Disable();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        bool jump = Input.GetKeyDown(KeyCode.Space);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(move.x, 0, move.y), Vector3.up);
            if (Quaternion.Angle(transform.rotation, targetRot) > 1f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime * 360f
                );
            }
        }

        if (move != lastSentMove || jump != lastSentJump)
        {
            Debug.Log($"[CLIENT] Predicting at {Time.time}, move: {move}, jump: {jump}");

            var input = new PlayerInputData(Time.time, move, jump);
            inputBuffer.Add(input);

            ApplyMovement(move);
            ApplyJump(jump);

            SendInputServerRpc(move, jump, Time.time);

            lastSentMove = move;
            lastSentJump = jump;
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        ApplyMovement(receivedMove);
        ApplyJump(receivedJump);
        ApplyGravity();
        AnimationHandler(receivedMove);

        receivedJump = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendInputServerRpc(Vector2 move, bool jump, float timestamp, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[SERVER] Received input from client at time {timestamp}, move: {move}, jump: {jump}");
        receivedMove = move;
        receivedJump = jump;

        Vector3 pos = rb.position;
        Quaternion rot = rb.rotation;
        Vector3 vel = rb.linearVelocity;

        SendReconcileClientRpc(pos, rot, vel, timestamp);
    }

    [ClientRpc]
    private void SendReconcileClientRpc(Vector3 serverPos, Quaternion serverRot, Vector3 serverVel, float serverTimestamp)
    {
        Debug.Log($"[CLIENT] Reconcile at {Time.time}, server timestamp: {serverTimestamp}, pos: {serverPos}");
        if (FindObjectOfType<DebugGhostSystem>() != null)
            FindObjectOfType<DebugGhostSystem>().UpdateServerPosition(serverPos);

        float dist = Vector3.Distance(rb.position, serverPos);
        if (dist > 0.25f)
        {
            rb.position = serverPos;
            rb.rotation = serverRot;
            rb.linearVelocity = serverVel;

            var replays = inputBuffer.Where(i => i.timestamp > serverTimestamp).ToList();
            foreach (var input in replays)
            {
                ApplyMovement(input.move);
                ApplyJump(input.jump);
            }
        }

        inputBuffer = inputBuffer.Where(i => i.timestamp > serverTimestamp).ToList();
    }

    private void ApplyMovement(Vector2 moveInput)
    {
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (IsServer && inputDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }

        if (inputDir.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        float multiplier = IsGrounded() ? 1f : 1f - airControlMultiplier;
        Vector3 move = inputDir.normalized * moveSpeed * multiplier;

        Vector3 velocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
        rb.linearVelocity = velocity;
    }



    private void ApplyJump(bool jump)
    {
        if (jump && IsGrounded() && Time.time >= lastJumpTime + jumpCooldown)
        {
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity= new Vector3(vel.x, jumpForce, vel.z);
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

    void AnimationHandler(Vector2 moveInput)
    {
        Vector3 velocity = rb.linearVelocity;
        bool isGrounded = IsGrounded();
        bool isTryingToMove = moveInput.magnitude > 0.1f;

        if (isGrounded && isTryingToMove)
        {
            animator.SetBool("isRunning", true);
        }
        else if (isGrounded && !isTryingToMove && animator.GetBool("isRunning"))
        {
            animator.SetBool("isRunning", false);
        }

        if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
        else if (velocity.y > 0.1f)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);
        }
        else if (velocity.y < -0.1f)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", true);
        }
    }
}
