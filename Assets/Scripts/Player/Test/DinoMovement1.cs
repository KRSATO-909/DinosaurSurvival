using UnityEngine;

public class DinoMovement1 : MonoBehaviour
{
    [Header("Скорости")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;

    [Header("Управление")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode biteKey = KeyCode.Mouse0; // ЛКМ = укус

    [Header("Ссылки")]
    public DinoAnim1 animScript; // Перетащи скрипт анимаций (опционально)

    // Приватные
    private CharacterController _cc;
    private Vector3 _velocity;
    private bool _isBiting;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (_isBiting) return; // Блокируем движение во время укуса

        // === Ввод ===
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool sprint = Input.GetKey(sprintKey);

        // === Поворот в сторону ввода ===
        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            Vector3 dir = new Vector3(h, 0, v);
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
        }

        // === Движение ===
        float speed = sprint ? runSpeed : walkSpeed;
        Vector3 move = new Vector3(h, 0, v) * speed;

        // === Гравитация ===
        if (!_cc.isGrounded)
            _velocity.y += -15f * Time.deltaTime;
        else
            _velocity.y = -1f;

        _cc.Move((move + _velocity) * Time.deltaTime);

        // === Укус ===
        if (Input.GetKeyDown(biteKey))
        {
            _isBiting = true;
            if (animScript != null) animScript.PlayBite();
        }
    }

    // Вызови эту функцию из Animation Event в конце анимации укуса!
    public void EndBite()
    {
        _isBiting = false;
    }

    // Для аниматора: текущая скорость 0..1
    public float GetSpeedRatio()
    {
        return Mathf.Clamp01(_cc.velocity.magnitude / runSpeed);
    }

    public bool IsGrounded() => _cc.isGrounded;
    public bool IsBiting() => _isBiting;
}
