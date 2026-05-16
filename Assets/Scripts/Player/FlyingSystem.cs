using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FlyingSystem : MonoBehaviour
{
    [Header("Takeoff / Landing")]
    [SerializeField] private float takeoffForce = 8f; // Сила взлёта
    [SerializeField] private float landingCheckDistance = 0.5f; // Дистанция проверки земли для посадки
    [SerializeField] private float minFlightHeight = 0.3f; // Минимальная высота полёта над землёй

    [Header("Flight Movement")]
    [SerializeField] private float flightSpeed = 8f;
    [SerializeField] private float fastFlightSpeed = 14f;
    [SerializeField] private float flightAcceleration = 5f;
    [SerializeField] private float pitchSpeed = 2f; // Скорость наклона вверх/вниз
    [SerializeField] private float yawSpeed = 3f; // Скорость поворота влево/вправо
    [SerializeField] private float bankAngle = 30f; // Угол крена при повороте

    [Header("Flight Physics")]
    [SerializeField] private float gravityInFlight = -2f; // Слабая гравитация в полёте
    [SerializeField] private float hoverForce = 5f; // Сила удержания высоты
    [SerializeField] private float maxHeight = 50f; // Максимальная высота

    private CharacterController controller;
    private PlayerMovement groundMovement;
    private AnimationController animController;
    private Animator animator;
    private Transform groundCheck;
    private LayerMask groundLayer;

    private bool isFlying = false;
    private bool isTakingOff = false; // Процесс взлёта
    private float currentFlightSpeed;
    private float verticalVelocity;
    private Vector3 flightDirection;

    // Хеши анимаций
    private int isFlyingHash;
    private int flightSpeedHash;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        groundMovement = GetComponent<PlayerMovement>();
        animController = GetComponent<AnimationController>();
        animator = GetComponent<Animator>();

        // Кешируем ссылки из PlayerMovement
        if (groundMovement != null)
        {
            groundCheck = groundMovement.groundCheck;
            groundLayer = groundMovement.groundLayer;
        }

        isFlyingHash = Animator.StringToHash("IsFlying");
        flightSpeedHash = Animator.StringToHash("FlightSpeed");

        flightDirection = transform.forward;
    }

    void Update()
    {
        if (animator == null) return;

        // Обработка взлёта
        if (!isFlying && Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            StartTakeoff();
        }

        // Обработка полёта
        if (isFlying || isTakingOff)
        {
            HandleFlight();
        }

        // Обновление аниматора
        animator.SetBool(isFlyingHash, isFlying || isTakingOff);
        if (isFlying)
        {
            animator.SetFloat(flightSpeedHash, currentFlightSpeed > flightSpeed * 0.5f ?
                (currentFlightSpeed > flightSpeed * 1.3f ? 2f : 1f) : 0f);
        }
    }

    void StartTakeoff()
    {
        isTakingOff = true;

        // Отключаем наземное движение
        if (groundMovement != null)
            groundMovement.enabled = false;

        // Начальный импульс вверх
        verticalVelocity = takeoffForce;
        flightDirection = transform.forward;
        currentFlightSpeed = flightSpeed * 0.5f;
    }

    void HandleFlight()
    {
        // Получаем ввод
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse X");

        bool isFastFlying = Input.GetKey(KeyCode.LeftShift);

        // Целевая скорость полёта
        float targetSpeed = isFastFlying ? fastFlightSpeed : flightSpeed;

        // Если игрок нажимает клавиши движения — ускоряемся
        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            currentFlightSpeed = Mathf.Lerp(currentFlightSpeed, targetSpeed, flightAcceleration * Time.deltaTime);
        }
        else
        {
            // Планирование — медленно замедляемся
            currentFlightSpeed = Mathf.Lerp(currentFlightSpeed, flightSpeed * 0.3f, Time.deltaTime);
        }

        // Поворот влево/вправо (от мыши)
        float yawRotation = mouseX * yawSpeed;
        transform.Rotate(Vector3.up, yawRotation);

        // Наклон вверх/вниз от Vertical
        float pitchRotation = -vertical * pitchSpeed * Time.deltaTime;

        // Вычисляем направление полёта (вперёд + небольшой наклон)
        flightDirection = transform.forward;
        flightDirection.y += pitchRotation * 0.1f;
        flightDirection.Normalize();

        // Управление высотой: Space — вверх, Ctrl — вниз
        float heightInput = 0f;
        if (Input.GetKey(KeyCode.Space))
            heightInput = 1f;
        else if (Input.GetKey(KeyCode.LeftControl))
            heightInput = -1f;

        // Гравитация в полёте
        verticalVelocity += gravityInFlight * Time.deltaTime;

        // Применяем подъёмную силу
        verticalVelocity += hoverForce * Time.deltaTime;

        // Ручное управление высотой
        verticalVelocity += heightInput * flightSpeed * 0.5f * Time.deltaTime;

        // Ограничение высоты
        if (transform.position.y > maxHeight && verticalVelocity > 0)
            verticalVelocity = 0;

        // Движение
        Vector3 moveDirection = flightDirection * currentFlightSpeed;
        moveDirection.y += verticalVelocity;

        controller.Move(moveDirection * Time.deltaTime);

        // Если на земле и фаза взлёта прошла — приземляемся
        if (IsGrounded() && !isTakingOff && verticalVelocity < 0)
        {
            Land();
        }

        // После взлёта переходим в режим полёта
        if (isTakingOff && transform.position.y > 0.5f && !IsGrounded())
        {
            isTakingOff = false;
            isFlying = true;
        }
    }

    void Land()
    {
        isFlying = false;
        isTakingOff = false;
        verticalVelocity = 0;
        currentFlightSpeed = 0;

        // Включаем наземное движение обратно
        if (groundMovement != null)
            groundMovement.enabled = true;
    }

    bool IsGrounded()
    {
        if (groundCheck == null) return false;

        return Physics.CheckSphere(groundCheck.position, 0.3f, groundLayer);
    }

    // Публичные методы для AnimationController
    public bool IsFlying()
    {
        return isFlying || isTakingOff;
    }

    public bool IsTakingOff()
    {
        return isTakingOff;
    }
}