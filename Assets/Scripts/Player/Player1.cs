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

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.8f;

    private CharacterController controller;
    private float verticalVelocity;
    private bool isGrounded;
    private bool isMoving;
    private bool isSprinting;
    private bool isAttacking;
    private float speedMultiplier = 1f;
    private float lastAttackTime;
    private bool attackProcessed;

    private Vector3 lastMoveDirection;
    private bool wasMovingBeforeFreeLook;

    private WaterBody[] allWaters;
    private float waterCheckTimer = 0f;

    private Vector3 GroundCheckCenter => groundCheck.position + groundCheckOffset;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        lastAttackTime = -attackCooldown;

        if (groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.parent = transform;
            go.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = go.transform;
        }

        if (attackPoint == null)
        {
            GameObject go = new GameObject("AttackPoint");
            go.transform.parent = transform;
            go.transform.localPosition = new Vector3(0, 0.5f, 1f);
            attackPoint = go.transform;
        }
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(GroundCheckCenter, groundCheckRadius, groundLayer);

        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0, vertical);
        Vector3 moveDirection = Vector3.zero;

        if (Camera.main != null)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            moveDirection = cameraForward * vertical + cameraRight * horizontal;
        }

        bool isFreeLooking = Input.GetMouseButton(1);

        if (isFreeLooking)
        {
            if (inputDirection.magnitude > 0.1f && !wasMovingBeforeFreeLook)
            {
                lastMoveDirection = moveDirection.normalized;
                wasMovingBeforeFreeLook = true;
            }
            else if (inputDirection.magnitude <= 0.1f)
            {
                wasMovingBeforeFreeLook = false;
            }

            if (wasMovingBeforeFreeLook && inputDirection.magnitude > 0.1f)
            {
                moveDirection = lastMoveDirection * inputDirection.magnitude;
            }
        }
        else
        {
            wasMovingBeforeFreeLook = false;
        }

        isMoving = moveDirection.magnitude > 0.1f;
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving;

        float speed = isSprinting ? sprintSpeed : walkSpeed;
        speed *= speedMultiplier;

        if (isMoving && !isFreeLooking && !isAttacking)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else if (isMoving && isFreeLooking && wasMovingBeforeFreeLook)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastMoveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveDirection.normalized * speed;
        velocity.y = verticalVelocity;

        // Проверка воды
        waterCheckTimer -= Time.deltaTime;
        if (waterCheckTimer <= 0f)
        {
            waterCheckTimer = 0.3f;
            allWaters = FindObjectsByType<WaterBody>(FindObjectsSortMode.None);
        }

        if (!canWalkUnderWater && allWaters != null)
        {
            foreach (WaterBody water in allWaters)
            {
                Vector3 groundCenter = GroundCheckCenter;

                // Если вода дошла до СЕРЕДИНЫ GroundCheck
                if (water.SurfaceY >= groundCenter.y)
                {
                    if (moveDirection.magnitude > 0.1f)
                    {
                        // Блокируем движение вперёд
                        velocity.x = 0f;
                        velocity.z = 0f;
                    }

                    // Не даём тонуть вниз
                    if (verticalVelocity < 0f)
                        verticalVelocity = 0f;

                    velocity.y = Mathf.Max(velocity.y, 0f);

                    break;
                }

                // Если ноги уже касаются воды — не даём проваливаться вниз
                Vector3 groundBottom = groundCenter + Vector3.down * groundCheckRadius;

                if (groundBottom.y <= water.SurfaceY)
                {
                    float targetY = water.SurfaceY + groundCheckRadius;

                    Vector3 pos = transform.position;
                    if (pos.y < targetY)
                    {
                        pos.y = targetY;
                        transform.position = pos;
                    }

                    if (verticalVelocity < 0f)
                        verticalVelocity = 0f;

                    velocity.y = Mathf.Max(velocity.y, 0f);
                }
            }
        }

        controller.Move(velocity * Time.deltaTime);

        if (isAttacking && !attackProcessed)
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

                if (stateInfo.normalizedTime >= 0.3f && stateInfo.normalizedTime <= 0.7f)
                {
                    PerformAttackDamage();
                    attackProcessed = true;
                }
            }
        }

        if (!isAttacking)
        {
            attackProcessed = false;
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

    public Vector3 GetGroundCheckCenter() => GroundCheckCenter;
    public float GetGroundCheckRadius() => groundCheckRadius;
    public LayerMask GetGroundLayer() => groundLayer;

    public void ResetVerticalVelocity()
    {
        verticalVelocity = 0f;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Time.time < lastAttackTime + attackCooldown ? Color.grey : Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GroundCheckCenter, groundCheckRadius);

        if (groundCheck != null && groundCheckOffset != Vector3.zero)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(groundCheck.position, GroundCheckCenter);
            Gizmos.DrawWireSphere(groundCheck.position, 0.05f);
        }
    }
}