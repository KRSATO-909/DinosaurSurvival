using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DinoController_Debug : MonoBehaviour
{
    [Header("🎮 Настройки движения")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float rotationSpeed = 10f;
    public float gravity = -20f;

    [Header("📷 Камера (опционально)")]
    public Transform cameraTransform; // Можно оставить пустым!
    public float mouseSensitivity = 2f;
    public float verticalLookLimit = 70f;

    [Header("🧪 Отладка")]
    public bool debugLogs = true;

    // Приватные переменные
    private CharacterController _cc;
    private Animator _animator;
    private Vector3 _velocity;
    private float _xRot, _yRot;
    private bool _isAttacking;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        // Проверки при старте
        if (_cc == null) Debug.LogError("❌ CharacterController НЕ найден!");
        if (_animator == null) Debug.LogWarning("⚠️ Animator НЕ найден — анимации не будут работать");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (debugLogs) Debug.Log($"✅ {name} инициализирован. Камера: {(cameraTransform ? cameraTransform.name : "НЕТ")}");
    }

    void Update()
    {
        // Блокировка ввода во время атаки
        if (_isAttacking) return;

        // === 1. ВВОД ===
        float h = Input.GetAxis("Horizontal"); // НЕ Raw — для плавности
        float v = Input.GetAxis("Vertical");
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool attack = Input.GetMouseButtonDown(0);

        // Лог ввода
        if (debugLogs && (h != 0 || v != 0 || sprint || attack))
            Debug.Log($"🎮 Ввод: H={h:F2}, V={v:F2}, Sprint={sprint}, Attack={attack}");

        // === 2. АТАКА ===
        if (attack)
        {
            TriggerAttack();
            return; // Прерываем кадр, чтобы не двигаться во время нажатия
        }

        // === 3. ВРАЩЕНИЕ КАМЕРЫ (если есть) ===
        if (cameraTransform != null)
        {
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

            _yRot += mx;
            _xRot = Mathf.Clamp(_xRot - my, -verticalLookLimit, verticalLookLimit);

            // Вращаем ТОЛЬКО камеру по вертикали, а игрока — по горизонтали
            cameraTransform.localRotation = Quaternion.Euler(_xRot, 0f, 0f);
            transform.rotation = Quaternion.Euler(0f, _yRot, 0f);
        }
        else
        {
            // Режим без камеры: вращение мышью всего объекта
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            _yRot += mx;
            transform.rotation = Quaternion.Euler(0f, _yRot, 0f);
        }

        // === 4. НАПРАВЛЕНИЕ ДВИЖЕНИЯ ===
        Vector3 forward = cameraTransform ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform ? cameraTransform.right : transform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 moveDir = (forward * v + right * h).normalized;

        // Поворот корпуса в сторону движения (только если есть ввод)
        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // === 5. СКОРОСТЬ И ГРАВИТАЦИЯ ===
        float speed = (sprint ? runSpeed : walkSpeed) * moveDir.magnitude;

        if (!_cc.isGrounded)
            _velocity.y += gravity * Time.deltaTime;
        else
            _velocity.y = -1f; // "Приклеивание" к земле

        // === 6. ПРИМЕНЕНИЕ ДВИЖЕНИЯ ===
        _cc.Move((moveDir * speed + _velocity) * Time.deltaTime);

        // === 7. АНИМАТОР (если есть) ===
        if (_animator != null)
        {
            float speedRatio = Mathf.Clamp01(_cc.velocity.magnitude / runSpeed);
            _animator.SetFloat("Speed", speedRatio, 0.1f, Time.deltaTime);
            _animator.SetBool("IsGrounded", _cc.isGrounded);
        }

        // === 8. ЛОГ состояния ===
        if (debugLogs && Time.frameCount % 60 == 0) // Каждую секунду
            Debug.Log($"📊 Скорость: {_cc.velocity.magnitude:F2}, Grounded: {_cc.isGrounded}, SpeedRatio: {Mathf.Clamp01(_cc.velocity.magnitude / runSpeed):F2}");
    }

    void TriggerAttack()
    {
        if (debugLogs) Debug.Log("⚔️ АТАКА!");

        _isAttacking = true;

        if (_animator != null)
        {
            _animator.SetTrigger("Attack");
            _animator.SetInteger("BiteVariant", Random.Range(1, 3));
        }
        else
        {
            // Если нет аниматора — просто завершаем атаку через 0.5 сек для теста
            Invoke(nameof(EndAttack), 0.5f);
        }
    }

    // Эту функцию вызывай через Animation Event в конце Bite1/Bite2
    public void EndAttack()
    {
        if (debugLogs) Debug.Log("✅ Атака завершена");
        _isAttacking = false;
        if (_animator != null) _animator.ResetTrigger("Attack");
    }

    // Визуализация в редакторе
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }
}