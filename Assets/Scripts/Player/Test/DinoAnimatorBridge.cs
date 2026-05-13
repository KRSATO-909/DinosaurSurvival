using UnityEngine;

public class DinoAnimatorBridge : MonoBehaviour
{
    [Header("🔗 Ссылки на другие модули")]
    public DinoMovementCore movement; // Перетащи сюда объект с движением (опционально)

    [Header("⚙️ Параметры Animator (должны совпадать!)")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string groundedParam = "IsGrounded";
    [SerializeField] private string attackParam = "Attack";
    [SerializeField] private string idleVariantParam = "IdleVariant";
    [SerializeField] private string biteVariantParam = "BiteVariant";

    [Header("🎲 Настройки рандома")]
    public float idleRerollChance = 0.02f; // 2% шанс сменить Idle в кадр
    public float minSpeedForWalk = 0.1f;
    public float minSpeedForRun = 0.6f;

    [Header("🧪 Отладка")]
    public bool debugLogs = true;

    private Animator _animator;
    private bool _isAttacking;

    // Публичный метод для вызова атаки извне (например, из UI или другого скрипта)
    public void TriggerAttack()
    {
        if (_animator == null) return;
        if (_isAttacking) return; // Защита от спама

        _isAttacking = true;
        _animator.SetTrigger(attackParam);
        _animator.SetInteger(biteVariantParam, Random.Range(1, 3));

        if (debugLogs) Debug.Log("⚔️ Анимация: Атака запущена");
    }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null) Debug.LogError("❌ DinoAnimator: Animator не найден!");
        if (debugLogs) Debug.Log("🎭 Аниматор инициализирован");
    }

    void Update()
    {
        if (_animator == null) return;

        // === 1. Получаем данные о движении ===
        float speed = 0f;
        bool isGrounded = true;

        if (movement != null)
        {
            // Нормализуем скорость: 0 = стоп, 0.5 = ходьба, 1.0 = бег
            speed = Mathf.Clamp01(movement.CurrentSpeed / (movement.runSpeed + 0.1f));
            isGrounded = movement.IsGrounded;
        }
        else
        {
            // Фолбэк: если movement не назначен, берём из собственного CharacterController
            var cc = GetComponent<CharacterController>();
            if (cc != null)
                speed = Mathf.Clamp01(cc.velocity.magnitude / 8f);
        }

        // === 2. Обновляем параметры Animator ===
        _animator.SetFloat(speedParam, speed, 0.1f, Time.deltaTime);
        _animator.SetBool(groundedParam, isGrounded);

        // === 3. Рандомный Idle при остановке ===
        if (speed < minSpeedForWalk && !_isAttacking)
        {
            if (Random.value < idleRerollChance)
            {
                int variant = Random.Range(1, 3);
                _animator.SetInteger(idleVariantParam, variant);
                if (debugLogs) Debug.Log($"🎲 IdleVariant = {variant}");
            }
        }

        // === 4. Лог для отладки ===
        if (debugLogs && Time.frameCount % 120 == 0)
            Debug.Log($"🎭 Animator: Speed={speed:F2}, Grounded={isGrounded}, Attacking={_isAttacking}");
    }

    // 📌 Эту функцию вызывай через Animation Event в конце Bite1/Bite2!
    public void EndAttack()
    {
        _isAttacking = false;
        if (_animator != null) _animator.ResetTrigger(attackParam);
        if (debugLogs) Debug.Log("✅ Анимация: Атака завершена");
    }
}
