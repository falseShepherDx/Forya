using System;
using System.Collections.Generic;
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

    public bool isGround;

    private float lastJumpTime = Mathf.NegativeInfinity;

    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true);

    private Queue<InputState> inputQueue = new Queue<InputState>();
    private List<InputState> inputHistory = new List<InputState>();
    private Vector3 lastServerPos;
    private int lastConfirmedTick = -1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerControls();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
       /* else
        {
            rb.isKinematic = true;         // physics engine'i kapat
            rb.useGravity = false;         // prediction yerçekimi elle yapılabilir
        }*/

        if (!IsOwner)
        {
            inputActions.Disable();
            enabled = false;
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
        //Debug.Log("UPDATE");
        if (!IsOwner) return;

        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        bool jump = Input.GetKeyDown(KeyCode.Space);

       // Debug.Log($"[INPUT CHECK] H: {Input.GetAxisRaw("Horizontal")}, V: {Input.GetAxisRaw("Vertical")}, Space: {jump}");


        InputState input = new InputState
        {
            tick = TickManager.CurrentTick,
            move = move,
            jump = jump
        };

        inputQueue.Enqueue(input);
        inputHistory.Add(input);

        SendInputServerRpc(input);
        ApplyInput(input);
        if (IsOwner && !IsServer)
        {
             // sadece client kendi ekranında prediction yapar
        }
       // Debug.Log($"[CLIENT INPUT] moveInput: {moveInput}, jumpPressed: {jumpPressed}");


    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (inputQueue.Count == 0) return;

       // Debug.Log($"[FIXED] InputQueue.Count: {inputQueue.Count}");

        InputState input = inputQueue.Dequeue();
        //Debug.Log($"[SERVER POS] Before Move: {transform.position}");

        //SendInputServerRpc(input);
    }

    private void ApplyInput(InputState input)
    {
        //Debug.Log($"[APPLY INPUT] Tick: {input.tick}, Move: {input.move}, Jump: {input.jump}");
        //Debug.Log($"[CLIENT APPLY] Tick: {input.tick}, Move: {input.move}, Jump: {input.jump}");
        if (!IsOwner && IsServer) return;
        Vector3 dir = new Vector3(input.move.x, 0, input.move.y).normalized;
        Debug.Log($"[CLIENT TRANSLATE] direction: {dir}, before: {transform.position}");
        transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);
        Debug.Log($"[CLIENT POS AFTER]: {transform.position}");

        // Debug.Log($"[CLIENT TRANSLATE] direction: {dir}, pos before: {transform.position}");

        //rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.deltaTime));
            animator.SetBool("isRunning", true);
            SetAnimBoolServerRpc("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
            SetAnimBoolServerRpc("isRunning", false);
        }

        if (input.jump && IsGrounded())
        {
            rb.linearVelocity= new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            lastJumpTime = Time.time;
            animator.SetBool("isJumping", true);
            SetAnimBoolServerRpc("isJumping", true);
        }
        else if (!IsGrounded() && rb.linearVelocity.y < -0.1f)
        {
            animator.SetBool("isFalling", true);
            SetAnimBoolServerRpc("isFalling", true);
        }
        else if (IsGrounded())
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            SetAnimBoolServerRpc("isJumping", false);
            SetAnimBoolServerRpc("isFalling", false);
        }
        //Debug.Log($"[SERVER POS] After Move: {transform.position}");

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

    [ServerRpc]
    private void SendInputServerRpc(InputState input)
    {
        Debug.Log($"[SERVER RPC] From Client: {OwnerClientId} | Move: {input.move} | Jump: {input.jump}");

        Vector3 position = transform.position;
        ConfirmPositionClientRpc(input.tick, position);
    }


    [ClientRpc]
    private void ConfirmPositionClientRpc(int confirmedTick, Vector3 serverPosition)
    {
        if (!IsOwner) return;
        //Debug.Log($"[CLIENT RPC] Confirmed Tick: {confirmedTick}, Server Pos: {serverPosition}");

        lastServerPos = serverPosition;
        lastConfirmedTick = confirmedTick;

        float distance = Vector3.Distance(transform.position, serverPosition);
        if (distance > 0.4f)
        {
            Debug.Log("fark çok var");
            // Sadece ciddi fark varsa pozisyonu düzelt
            transform.position = serverPosition;

            int index = inputHistory.FindIndex(i => i.tick == confirmedTick);
            if (index >= 0)
            {
                inputHistory.RemoveRange(0, index + 1);

                foreach (var input in inputHistory)
                {
                    ApplyInput(input); // sadece kalanları tekrar uygula
                }
            }
        }
        else
        {
            // Fark çok küçük → sadece geçmişi temizle
            int index = inputHistory.FindIndex(i => i.tick == confirmedTick);
            if (index >= 0)
            {
                inputHistory.RemoveRange(0, index + 1);
            }
        }
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

        // Tüm clientlarda ses çalmak için AudioSource.PlayClipAtPoint kullan
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, pos);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetAnimBoolServerRpc(string param, bool value)
    {
        SetAnimBoolClientRpc(param, value);
    }

    [ClientRpc]
    void SetAnimBoolClientRpc(string param, bool value)
    {
        if (animator == null)
        {
            Debug.LogError($"Animator is NULL on client {OwnerClientId}!");
            return;
        }
        animator.SetBool(param, value);
    }
}


