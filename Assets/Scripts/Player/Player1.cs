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
    public LayerMask groundLayer;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public float attackCooldown = 0.8f; // Кулдаун вернулся сюда!

    private CharacterController controller;
    private float verticalVelocity;
    private bool isGrounded;
    private bool isMoving;
    private bool isSprinting;
    private bool isAttacking;
    private float speedMultiplier = 1f;
    private float lastAttackTime;
    private bool attackProcessed;

    // Для ПКМ: запоминаем направление движения
    private Vector3 lastMoveDirection;
    private bool wasMovingBeforeFreeLook;

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
        // Проверка на земле
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        // Движение
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0, vertical);

        // Получаем направление относительно камеры
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

        // Логика ПКМ: запоминаем направление когда начинаем свободный обзор
        if (isFreeLooking)
        {
            // Если только что нажали ПКМ и двигались — запоминаем направление
            if (inputDirection.magnitude > 0.1f && !wasMovingBeforeFreeLook)
            {
                lastMoveDirection = moveDirection.normalized;
                wasMovingBeforeFreeLook = true;
            }
            // Если стоим на месте — обнуляем
            else if (inputDirection.magnitude <= 0.1f)
            {
                wasMovingBeforeFreeLook = false;
            }

            // Используем сохранённое направление
            if (wasMovingBeforeFreeLook && inputDirection.magnitude > 0.1f)
            {
                moveDirection = lastMoveDirection * inputDirection.magnitude;
            }
        }
        else
        {
            // Отпустили ПКМ — сбрасываем
            wasMovingBeforeFreeLook = false;
        }

        // Определяем состояние движения
        isMoving = moveDirection.magnitude > 0.1f;
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving;

        // Скорость
        float speed = isSprinting ? sprintSpeed : walkSpeed;
        speed *= speedMultiplier;

        // Поворот в сторону движения
        // ВАЖНО: поворачиваем только если двигаемся и не в свободном обзоре
        if (isMoving && !isFreeLooking && !isAttacking)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        // Если в свободном обзоре — продолжаем смотреть в lastMoveDirection
        else if (isMoving && isFreeLooking && wasMovingBeforeFreeLook)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Гравитация
        verticalVelocity += gravity * Time.deltaTime;

        // Движение
        Vector3 velocity = moveDirection.normalized * speed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        // Обработка урона атаки
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
        Debug.Log("Укус!");

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                Debug.Log("Попал в: " + hit.name);
            }
        }
    }

    // Публичные методы
    public bool IsMoving() => isMoving;
    public bool IsSprinting() => isSprinting;
    public bool IsGrounded() => isGrounded;
    public bool IsFreeLooking() => Input.GetMouseButton(1);

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public void OnAttackStarted()
    {
        lastAttackTime = Time.time;
    }

    public void SetAttacking(bool attacking)
    {
        isAttacking = attacking;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Time.time < lastAttackTime + attackCooldown ? Color.grey : Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}