using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float rotationSpeed = 10f;
    public float gravity = -20f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public Vector3 groundCheckOffset = Vector3.zero;
    public LayerMask groundLayer;

    [Header("Water Interaction")]
    public bool canWalkUnderWater = true;
    public float waterBlockDistance = 3f;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.8f;

    private CharacterController controller;
    private Animator animator;

    private float verticalVelocity;
    private float speedMultiplier = 1f;
    private float lastAttackTime;

    private bool isGrounded;
    private bool isMoving;
    private bool isSprinting;
    private bool isAttacking;
    private bool attackProcessed;
    private bool wasMovingBeforeFreeLook;

    private Vector3 lastMoveDirection;

    private Vector3 GroundCheckCenter => groundCheck.position + groundCheckOffset;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        lastAttackTime = -attackCooldown;

        if (groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = go.transform;
        }

        if (attackPoint == null)
        {
            GameObject go = new GameObject("AttackPoint");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0, 0.5f, 1f);
            attackPoint = go.transform;
        }
    }

    void Update()
    {
        HandleGroundCheck();

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);
        Vector3 moveDirection = GetCameraRelativeMove(horizontal, vertical);

        bool isFreeLooking = Input.GetMouseButton(1);

        HandleFreeLook(ref moveDirection, inputDirection, isFreeLooking);

        isMoving = moveDirection.sqrMagnitude > 0.01f;
        isSprinting = isMoving && Input.GetKey(KeyCode.LeftShift);

        float speed = (isSprinting ? sprintSpeed : walkSpeed) * speedMultiplier;

        HandleRotation(moveDirection, isFreeLooking);

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 horizontalVelocity = isMoving
            ? moveDirection.normalized * speed
            : Vector3.zero;

        if (!canWalkUnderWater && isMoving)
        {
            horizontalVelocity = ResolveWaterMovement(moveDirection.normalized, speed);
        }

        Vector3 velocity = horizontalVelocity;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        HandleAttack();
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(
            GroundCheckCenter,
            groundCheckRadius,
            groundLayer
        );

        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
    }

    Vector3 GetCameraRelativeMove(float horizontal, float vertical)
    {
        if (Camera.main == null)
            return Vector3.zero;

        Transform cam = Camera.main.transform;

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return forward * vertical + right * horizontal;
    }

    void HandleFreeLook(ref Vector3 moveDirection, Vector3 inputDirection, bool isFreeLooking)
    {
        bool hasInput = inputDirection.sqrMagnitude > 0.01f;

        if (!isFreeLooking)
        {
            wasMovingBeforeFreeLook = false;
            return;
        }

        if (hasInput && !wasMovingBeforeFreeLook)
        {
            lastMoveDirection = moveDirection.normalized;
            wasMovingBeforeFreeLook = true;
        }
        else if (!hasInput)
        {
            wasMovingBeforeFreeLook = false;
        }

        if (wasMovingBeforeFreeLook && hasInput)
        {
            moveDirection = lastMoveDirection * inputDirection.magnitude;
        }
    }

    void HandleRotation(Vector3 moveDirection, bool isFreeLooking)
    {
        if (!isMoving || isAttacking)
            return;

        Vector3 lookDirection = moveDirection;

        if (isFreeLooking && wasMovingBeforeFreeLook)
            lookDirection = lastMoveDirection;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    Vector3 ResolveWaterMovement(Vector3 desiredDir, float speed)
    {
        if (desiredDir.sqrMagnitude < 0.001f)
            return Vector3.zero;

        Vector3 currentPos = transform.position;
        Vector3 rayStart = currentPos + Vector3.up * 0.5f;
        Vector3 rayDir = (desiredDir + Vector3.down * 0.5f).normalized;

        Debug.DrawRay(rayStart, rayDir * waterBlockDistance, Color.cyan);

        if (Physics.Raycast(rayStart, rayDir, out RaycastHit hit, waterBlockDistance))
        {
            WaterBody water = hit.collider.GetComponent<WaterBody>();

            if (water != null && water.SurfaceY < currentPos.y - 0.2f)
                return Vector3.zero;
        }

        return desiredDir * speed;
    }

    void HandleAttack()
    {
        if (!isAttacking)
        {
            attackProcessed = false;
            return;
        }

        if (attackProcessed || animator == null)
            return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.normalizedTime >= 0.3f && stateInfo.normalizedTime <= 0.7f)
        {
            PerformAttackDamage();
            attackProcessed = true;
        }
    }

    void PerformAttackDamage()
    {
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                Debug.Log("Попал в: " + hit.name);
            }
        }
    }

    public bool IsMoving() => isMoving;
    public bool IsSprinting() => isSprinting;
    public bool IsGrounded() => isGrounded;
    public bool IsFreeLooking() => Input.GetMouseButton(1);

    public bool CanAttack() => Time.time >= lastAttackTime + attackCooldown;

    public void OnAttackStarted() => lastAttackTime = Time.time;
    public void SetAttacking(bool attacking) => isAttacking = attacking;
    public void SetSpeedMultiplier(float multiplier) => speedMultiplier = multiplier;
    public void ResetVerticalVelocity() => verticalVelocity = 0f;

    public Vector3 GetGroundCheckCenter() => GroundCheckCenter;
    public float GetGroundCheckRadius() => groundCheckRadius;
    public LayerMask GetGroundLayer() => groundLayer;

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Time.time < lastAttackTime + attackCooldown
                ? Color.grey
                : Color.red;

            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GroundCheckCenter, groundCheckRadius);
    }
}