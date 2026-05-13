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
    public float attackCooldown = 0.5f;

    private CharacterController controller;
    private float verticalVelocity;
    private bool isGrounded;
    private float lastAttackTime;
    private bool isMoving;
    private bool isSprinting;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Если groundCheck не назначен, создаем автоматически
        if (groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.parent = transform;
            go.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = go.transform;
        }

        // Если attackPoint не назначен, создаем автоматически
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

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical);

        // Получаем направление относительно камеры
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

        // Определяем состояние движения
        isMoving = moveDirection.magnitude > 0.1f;
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving;

        // Скорость
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        // Поворот в сторону движения (только если не зажат ПКМ и не атакуем)
        if (isMoving && !Input.GetMouseButton(1))
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Гравитация
        verticalVelocity += gravity * Time.deltaTime;

        // ОДИН вызов Move - объединяем движение и гравитацию
        Vector3 velocity = moveDirection.normalized * speed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        // Атака с кулдауном
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        Debug.Log("Укус!");

        // Проверка попадания
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                Debug.Log("Попал в: " + hit.name);
            }
        }
    }

    // Публичные методы для AnimationController
    public bool IsMoving()
    {
        return isMoving;
    }

    public bool IsSprinting()
    {
        return isSprinting;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsFreeLooking()
    {
        return Input.GetMouseButton(1);
    }

    // Визуализация для отладки
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            // Кулдаун визуализация
            if (Time.time < lastAttackTime + attackCooldown)
                Gizmos.color = Color.grey;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}