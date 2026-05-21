using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FlyingSystem : MonoBehaviour
{
    [Header("Takeoff")]
    [SerializeField] private float takeoffJumpHeight = 5f; // Высота прыжка-взлёта

    [Header("Flight Movement")]
    [SerializeField] private float flightSpeed = 8f;
    [SerializeField] private float fastFlightSpeed = 14f;
    [SerializeField] private float flightAcceleration = 5f;

    [Header("Flight Physics")]
    [SerializeField] private float gravityInFlight = -2f;
    [SerializeField] private float hoverForce = 5f;

    private CharacterController controller;
    private PlayerMovement groundMovement;
    private Animator animator;
    private Transform groundCheck;
    private LayerMask groundLayer;
    private float groundCheckRadius;

    // Состояния
    private enum FlightState { Grounded, TakingOff, Flying, Landing }
    private FlightState state = FlightState.Grounded;

    private float currentFlightSpeed;
    private float verticalVelocity;
    private Vector3 flightDirection; // Направление полёта (горизонтальное)

    // Таймеры и триггеры
    private float stateTimer;
    private bool takeoffTriggered;
    private bool landingTriggered;

    // Хеши
    private int isFlyingHash;
    private int flightSpeedHash;
    private int takeoffTriggerHash;
    private int landingTriggerHash;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        groundMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        if (groundMovement != null)
        {
            groundCheck = groundMovement.groundCheck;
            groundLayer = groundMovement.groundLayer;
            groundCheckRadius = groundMovement.groundCheckRadius;
        }

        isFlyingHash = Animator.StringToHash("IsFlying");
        flightSpeedHash = Animator.StringToHash("FlightSpeed");
        takeoffTriggerHash = Animator.StringToHash("Takeoff");
        landingTriggerHash = Animator.StringToHash("Land");

        flightDirection = transform.forward;
        Debug.Log("[FlyingSystem] Инициализация завершена");
    }

    void Update()
    {
        Debug.Log($"[FlyingSystem] State={state}, IsGrounded={IsGrounded()}, verticalVelocity={verticalVelocity:F2}");

        switch (state)
        {
            case FlightState.Grounded:
                HandleGrounded();
                break;
            case FlightState.TakingOff:
                HandleTakingOff();
                break;
            case FlightState.Flying:
                HandleFlying();
                break;
            case FlightState.Landing:
                HandleLanding();
                break;
        }

        UpdateAnimatorParameters();
    }

    void HandleGrounded()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Debug.Log("[FlyingSystem] Запуск взлёта");
            StartTakeoff();
        }
    }

    void StartTakeoff()
    {
        state = FlightState.TakingOff;
        stateTimer = 0f;
        takeoffTriggered = true;

        if (groundMovement != null)
            groundMovement.enabled = false;

        // Импульс вверх
        verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravityInFlight) * takeoffJumpHeight);
        flightDirection = transform.forward; // Направление вперёд
        currentFlightSpeed = 0f;

        Debug.Log($"[FlyingSystem] Взлёт: начальная скорость вверх = {verticalVelocity:F2}, высота прыжка = {takeoffJumpHeight}");

        if (animator != null)
        {
            animator.SetTrigger(takeoffTriggerHash);
            animator.SetBool(isFlyingHash, true);
            Debug.Log("[FlyingSystem] Триггер Takeoff отправлен");
        }
    }

    void HandleTakingOff()
    {
        // Гравитация во время взлёта
        verticalVelocity += gravityInFlight * Time.deltaTime;

        Vector3 move = Vector3.up * verticalVelocity;
        // Небольшое движение вперёд по инерции
        move += flightDirection * 0.5f;
        controller.Move(move * Time.deltaTime);

        Debug.Log($"[FlyingSystem] Взлёт: высота={transform.position.y:F2}, верт.скорость={verticalVelocity:F2}");

        // Как только скорость стала отрицательной – взлёт окончен
        if (verticalVelocity <= 0f)
        {
            Debug.Log("[FlyingSystem] Вершина взлёта достигнута, переход в полёт");
            state = FlightState.Flying;
            verticalVelocity = 0f;
            currentFlightSpeed = flightSpeed * 0.3f; // начинаем с небольшой скорости
        }
    }

    void HandleFlying()
    {
        // Ввод направления
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Направление относительно камеры (как на земле)
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 inputDir = (camForward * vertical + camRight * horizontal).normalized;

        // Если есть ввод – летим туда, иначе сохраняем текущее направление (инерция)
        if (inputDir.magnitude > 0.1f)
        {
            flightDirection = Vector3.Slerp(flightDirection, inputDir, 5f * Time.deltaTime).normalized;
        }
        // else flightDirection остаётся прежним

        // Скорость полёта
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? fastFlightSpeed : flightSpeed;
        bool isMoving = inputDir.magnitude > 0.1f;

        if (isMoving)
            currentFlightSpeed = Mathf.Lerp(currentFlightSpeed, targetSpeed, flightAcceleration * Time.deltaTime);
        else
            currentFlightSpeed = Mathf.Lerp(currentFlightSpeed, 0.2f * flightSpeed, 2f * Time.deltaTime);

        // Управление высотой
        float heightInput = 0f;
        if (Input.GetKey(KeyCode.Space)) heightInput = 1f;
        else if (Input.GetKey(KeyCode.LeftControl)) heightInput = -1f;

        // Физика
        verticalVelocity += gravityInFlight * Time.deltaTime;
        verticalVelocity += hoverForce * Time.deltaTime;
        verticalVelocity += heightInput * flightSpeed * 0.5f * Time.deltaTime;

        // Без ограничения максимальной высоты!

        // Итоговое перемещение
        Vector3 move = flightDirection * currentFlightSpeed;
        move.y += verticalVelocity;
        controller.Move(move * Time.deltaTime);

        Debug.Log($"[FlyingSystem] Полёт: высота={transform.position.y:F2}, скорость={currentFlightSpeed:F2}, верт.скорость={verticalVelocity:F2}, направление={flightDirection}");

        // Посадка: Ctrl + коснулись земли
        if (Input.GetKey(KeyCode.LeftControl) && IsGrounded() && verticalVelocity < -0.5f)
        {
            Debug.Log("[FlyingSystem] Инициирована посадка");
            StartLanding();
        }
    }

    void StartLanding()
    {
        state = FlightState.Landing;
        stateTimer = 0f;
        landingTriggered = true;
        currentFlightSpeed = 0f;
        verticalVelocity = 0f;

        if (animator != null)
        {
            animator.SetTrigger(landingTriggerHash);
            animator.SetBool(isFlyingHash, false);
            Debug.Log("[FlyingSystem] Триггер Land отправлен");
        }
    }

    void HandleLanding()
    {
        stateTimer += Time.deltaTime;

        // Ждём окончания анимации посадки
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("LandingQetz") && stateInfo.normalizedTime >= 0.9f)
            {
                Debug.Log("[FlyingSystem] Анимация посадки завершена");
                CompleteLanding();
            }
        }
        else if (stateTimer > 1.5f) // запасной выход
        {
            CompleteLanding();
        }
    }

    void CompleteLanding()
    {
        state = FlightState.Grounded;
        Debug.Log("[FlyingSystem] Посадка завершена, возврат на землю");

        if (groundMovement != null)
            groundMovement.enabled = true;

        if (animator != null)
        {
            animator.SetBool(isFlyingHash, false);
            animator.SetFloat(flightSpeedHash, 0f);
        }
    }

    void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        // Плавное изменение FlightSpeed с гистерезисом
        if (state == FlightState.Flying)
        {
            float targetFs;
            if (currentFlightSpeed > flightSpeed * 1.3f)
                targetFs = 2f;
            else if (currentFlightSpeed > 0.5f)
                targetFs = 1f;
            else
                targetFs = 0f;

            float currentFs = animator.GetFloat(flightSpeedHash);
            float newFs = Mathf.MoveTowards(currentFs, targetFs, 3f * Time.deltaTime);
            animator.SetFloat(flightSpeedHash, newFs);
        }
        else if (state == FlightState.Grounded || state == FlightState.Landing)
        {
            animator.SetFloat(flightSpeedHash, 0f);
        }
    }

    bool IsGrounded()
    {
        if (groundCheck == null) return false;
        bool grounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        return grounded;
    }

    public bool IsFlying()
    {
        return state == FlightState.TakingOff || state == FlightState.Flying || state == FlightState.Landing;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}