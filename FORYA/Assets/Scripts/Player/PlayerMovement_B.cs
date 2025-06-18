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
    private PlayerControls inputActions;
    private Rigidbody rb;
    private float lastJumpTime = Mathf.NegativeInfinity;
    private Vector2 moveInput;
    private bool jumpInput;
    private AudioSource audioSource;
    [SerializeField] Animator animator;
    public bool isGround;
    [Header("VFX and SFXs")]
    [SerializeField] GameObject deathParticle;
    [SerializeField] private AudioClip deathSound;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();    
        inputActions = new PlayerControls();
        audioSource = GetComponent<AudioSource>();
       
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            ScreenCapture.CaptureScreenshot("screenshot_" + Time.time + ".png");
        }
    }

    public override void OnNetworkSpawn()
    {
        // Sadece server pozisyonu ayarlamal� (host da dahil)
        if (IsServer)
        {
            Vector3 spawnPos = SpawnManager.instance.GetSpawnPointForClient(OwnerClientId);
            transform.position = spawnPos;

            // Fizik sapmas�n� engelle
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

        }

        // E�er bu obje yerel oyuncuya ait de�ilse, hareket ve input i�lemleri kapat�l�r
        if (!IsOwner)
        {
            rb.isKinematic = true;
            enabled = false;
           
            return;
        }

        
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Movement.performed += OnMove;
        inputActions.Player.Movement.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Movement.performed -= OnMove;
        inputActions.Player.Movement.canceled -= OnMove;
        inputActions.Player.Jump.performed -= OnJump;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        jumpInput = true;
    }

    private void FixedUpdate()
    {
        CustomGravity();
        HandleMovement();
        HandleJump();
        AnimationHandler();
    }

    private void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            //move   
            var controlMultiplier = IsGrounded() ? 1f : 1-airControlMultiplier; // change the speed based on players grounded state.
            Vector3 move = inputDirection.normalized * moveSpeed * controlMultiplier*Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

           //rotate
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up); 
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    private void HandleJump()
    {
        if (jumpInput && IsGrounded() && Time.time >= lastJumpTime + jumpCooldown) 
        {
            Vector3 currentVelocity = rb.linearVelocity;
            rb.linearVelocity = new Vector3(currentVelocity.x, jumpForce, currentVelocity.z);
            lastJumpTime = Time.time; 
        }

        jumpInput = false;
    }
    private void CustomGravity()
    {
        if (!IsGrounded())
        {
            Vector3 gravity = Physics.gravity * gravityMultiplier;
            rb.AddForce(gravity, ForceMode.Acceleration);
        }
    }

    private bool IsGrounded()
    {
        
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance, groundLayer);
    }
   

    [ServerRpc(RequireOwnership =false)]
    public void DeadServerRPC()
    {
        ShowDeathEffectClientRPC(transform.position);
        GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    void ShowDeathEffectClientRPC(Vector3 transform)
    {
        if (deathParticle != null)
        {
            Instantiate(deathParticle, transform, Quaternion.identity);
        }

        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("deadLine"))
        {
            DeadServerRPC();
         
        }
    }
    

    void AnimationHandler()
    {
        Vector3 velocity = rb.linearVelocity;
        bool isGrounded = IsGrounded();

        // Kullanıcının bastığı input'a göre hareket kontrolü
        bool isTryingToMove = moveInput.magnitude > 0.1f;

        // Koşma animasyonu
        if (isGrounded && isTryingToMove)
        {
            animator.SetBool("isRunning", true);
        }
        // Koşmayı bırakma → RunStop
        else if (isGrounded && !isTryingToMove && animator.GetBool("isRunning"))
        {
            animator.ResetTrigger("isRunStopping");
            animator.SetTrigger("isRunStopping");
            animator.SetBool("isRunning", false);
        }

        // Yerde ve hareket etmiyorsa zıplama/düşme iptal
        if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }

        // Havada zıplama (yukarı çıkış)
        if (!isGrounded && velocity.y > 0.1f)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);
        }
        // Havada düşüş (aşağı iniş)
        else if (!isGrounded && velocity.y < -0.1f)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", true);
        }
    }



}
