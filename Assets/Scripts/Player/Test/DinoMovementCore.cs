using UnityEngine;

public class DinoMovementCore : MonoBehaviour
{
    [Header("🎮 Ввод")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("⚡ Скорости")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("🌍 Физика")]
    public float gravity = -20f;
    public float groundStickForce = -2f; // "Прилипание" к земле

    [Header("🧪 Отладка")]
    public bool debugLogs = true;

    // Приватные
    private CharacterController _cc;
    private Vector3 _velocity;
    private bool _isSprinting;

    // Публичные свойства (для других скриптов)
    public bool IsGrounded => _cc.isGrounded;
    public float CurrentSpeed => _cc.velocity.magnitude;
    public bool IsMoving => _cc.velocity.magnitude > 0.1f;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (_cc == null) Debug.LogError("❌ DinoMovement: CharacterController не найден!");
        if (debugLogs) Debug.Log($"🦖 Движение инициализировано: {_cc.name}");
    }

    void Update()
    {
        // === 1. Чтение ввода ===
        float h = Input.GetAxis(horizontalAxis);
        float v = Input.GetAxis(verticalAxis);
        _isSprinting = Input.GetKey(sprintKey);

        // Лог ввода
        if (debugLogs && (h != 0 || v != 0 || _isSprinting) && Time.frameCount % 30 == 0)
            Debug.Log($"🎮 Ввод: H={h:F2}, V={v:F2}, Sprint={_isSprinting}");

        // === 2. Направление движения (в мировом пространстве) ===
        Vector3 moveDir = new Vector3(h, 0, v).normalized;

        // === 3. Поворот динозавра в сторону движения ===
        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // === 4. Гравитация ===
        if (!_cc.isGrounded)
            _velocity.y += gravity * Time.deltaTime;
        else
            _velocity.y = groundStickForce; // Чтобы не "прыгал" на земле

        // === 5. Применение движения ===
        float speed = _isSprinting ? runSpeed : walkSpeed;
        _cc.Move((moveDir * speed + _velocity) * Time.deltaTime);

        // === 6. Лог состояния ===
        if (debugLogs && Time.frameCount % 60 == 0)
            Debug.Log($"📊 Speed={_cc.velocity.magnitude:F2}, Grounded={_cc.isGrounded}, Sprint={_isSprinting}");
    }

    // Визуализация в редакторе
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.5f);
    }
}
